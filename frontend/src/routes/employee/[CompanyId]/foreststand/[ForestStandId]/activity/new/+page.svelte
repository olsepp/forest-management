<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/employee/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type {
		CadasterSummaryDto,
		ForestStandSummaryDto
	} from '$lib/dtos/forest-stand/forest-stand.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let forestStand = $state<ForestStandSummaryDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);
	const companyId = $derived($page.params.CompanyId ?? '');
	const forestStandId = $derived($page.params.ForestStandId ?? '');

	async function loadCadasterPropertyFallback(cadasterId: string, token: string): Promise<void> {
		const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
			headers: { Authorization: `Bearer ${token}` }
		});

		if (!response.ok || !forestStand) return;

		const cadaster = (await response.json()) as CadasterSummaryDto;
		forestStand = {
			...forestStand,
			landPropertyId: cadaster.landPropertyId ?? forestStand.landPropertyId,
			landPropertyName: cadaster.landPropertyName ?? forestStand.landPropertyName
		};
	}

	async function loadForestStandSummary() {
		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const forestStandId = $page.params.ForestStandId;
			if (!forestStandId) {
				errorMessage = 'Puudub eraldise ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/foreststands/${forestStandId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage =
					response.status === 404 ? 'Eraldist ei leitud.' : 'Eraldise laadimine ebaõnnestus.';
				return;
			}

			const dto = (await response.json()) as ForestStandSummaryDto;
			forestStand = {
				id: dto.id,
				number: dto.number,
				cadasterId: dto.cadasterId,
				cadasterCadastralNumber: dto.cadasterCadastralNumber,
				landPropertyId: dto.landPropertyId,
				landPropertyName: dto.landPropertyName
			};

			if (forestStand && (!forestStand.landPropertyId || !forestStand.landPropertyName)) {
				await loadCadasterPropertyFallback(forestStand.cadasterId, token);
			}
		} catch {
			errorMessage = 'Eraldise laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadForestStandSummary);
</script>

<p class="employee-back-link">
	<a
		class="employee-back-link-button"
		href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
			CompanyId: companyId,
			ForestStandId: forestStandId
		})}
	>
		<span aria-hidden="true">←</span>
		<span>Tagasi eraldise juurde</span>
	</a>
</p>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse eraldist…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if forestStand}
	<section class="employee-card summary">
		<p><strong>Eraldis:</strong> Eraldis {forestStand.number}</p>
		<p>
			<strong>Kataster:</strong>
			<a
				href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
					CompanyId: companyId,
					CadasterId: forestStand.cadasterId
				})}>{forestStand.cadasterCadastralNumber}</a
			>
		</p>
		{#if forestStand.landPropertyId && forestStand.landPropertyName}
			<p>
				<strong>Kinnistu:</strong>
				<a
					href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: companyId,
						LandPropertyId: forestStand.landPropertyId
					})}>{forestStand.landPropertyName}</a
				>
			</p>
		{/if}
	</section>

	<ActivityForm
		{companyId}
		cadasterId={forestStand.cadasterId}
		cadasterLabel={forestStand.cadasterCadastralNumber}
		forestStandId={forestStand.id}
		lockCadaster={true}
		cancelHref={`/employee/${companyId}/foreststand/${forestStand.id}`}
		redirectHref={`/employee/${companyId}/foreststand/${forestStand.id}`}
		submitLabel="Logi tegevus"
	/>
{/if}

<style>
	.summary {
		margin-bottom: 0.85rem;
	}

	.summary p {
		margin: 0.3rem 0;
		color: #334155;
	}

	.summary a {
		color: #1f5a42;
		font-weight: 700;
		text-decoration: none;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}
</style>
