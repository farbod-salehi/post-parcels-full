<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
            <div>
               فهرست پکیج‌ها
            </div>
            <div>
               
                <router-link :to="{ name: 'parcelItem' }">
                    ثبت پکیج جدید
                </router-link>
            </div>
        </div>
    </div>
    <div v-if="initLoading" style="text-align:center;">
        <div class="spinner-border text-secondary" role="status">
            <span class="visually-hidden">Loading...</span>
        </div>
    </div>

    <div v-else>

        <div v-show="messages.length > 0" class="alert alert-danger alert-dismissible fade show" role="alert">
            <div v-for="mes of messages" :key="mes" class="mt-1">
                - {{ mes }}
            </div>
            <button type="button" class="btn-close" aria-label="Close" @click="clearMessages"></button>
        </div>

        <div style="margin-top: 30px; margin-bottom: 50px;">
            <input v-model="searchQuery" @keyup.enter="getList(1)" type="text" class="form-control rtl"
                style="display: inline; width:60%;" id="searchQuery" placeholder="جستجو با شناسه مرسوله یا کد پکیج" />
            &nbsp;
            <button class="btn btn-primary btn-sm" @click="getList(1)">
                <b-icon-search />
                جستجو
            </button>
        </div>
        <table v-if="grid_pages_count > 0" class="table table-sm table-striped" style="width:100%">
            <thead style="text-align:center">
                <tr>
                    <th scope="col"> ردیف </th>
                    <th scope="col">کد پکیج</th>
                    <th scope="col"> تعداد مرسوله</th>
                    <th scope="col">تاریخ ثبت</th>

                    <th scope="col"></th>
                </tr>
            </thead>
            <tbody style="text-align:center; font-size: 0.9em;">
                <tr v-for="(parcel,index) of parcelsList" :key="parcel.id">
                    <th scope="row">
                        {{ ((grid_current_page - 1) * Number(grid_page_size)) + (index + 1) }}
                       
                    </th>

                    <td>
                        {{ parcel.code }}
                    </td>
                    <td>
                       
                       {{ parcel.itemsCount }}
                    </td>
                    <td style="direction: ltr;">
                        {{ parcel.createdAt }}
                    </td>

                    <td>
                        <router-link
                            :to="{ name: 'parcelItem',params: { editId: parcel.id } }">
                            ویرایش
                        </router-link>
                        &nbsp;
                        |
                        &nbsp;
                        <router-link
                            :to="{ name: 'parcelItemDocuments',params: { editId: parcel.id } }">
                            مستندات
                        </router-link>
                        &nbsp;
                        |
                        &nbsp;
                        <a href="#" @click="openReport(parcel.id)">
                            چاپ
                        </a>
                    </td>
                </tr>
            </tbody>
        </table>
        <div v-if="grid_pages_count === 0 && initLoading === false">
            <div class="alert alert-info sm" role="alert">
                داده‌ای برای نمایش یافت نشد.
            </div>
        </div>
        <div v-if="grid_pages_count > 0" style="text-align: center; margin-top:40px;">

            <b-icon-arrow-right-circle-fill @click="getList(grid_current_page - 1)"
                :class="['button-icon',grid_current_page === 1 ? 'disabled' : '']" />
            &nbsp;
            <span>
                {{ grid_current_page }}
            </span>
            &nbsp;
            از
            &nbsp;
            <span>
                {{ grid_pages_count }}
            </span>
            &nbsp;
            <b-icon-arrow-left-circle-fill @click="getList(grid_current_page + 1)"
                :class="['button-icon',grid_current_page === grid_pages_count ? 'disabled' : '']" />
        </div>

    </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';
import { sendRequest, requestErrorHandling, baseAPI } from './../../utility';

export default {
    name: 'ParcelListForm',
    setup() {

        const router = useRouter();

        const store = useStore();

        let messages = ref([]);

        let initLoading = ref(false);
        let parcelsList = ref([]);
        const grid_page_size = 20;
        let grid_pages_count = ref(1);
        let grid_current_page = ref(1);
        let searchQuery = ref('');


        onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.
            await getList(1);
        });

        async function getList(pageNumber) {
            if (pageNumber <= 0 || (grid_pages_count.value !== 0 && pageNumber > grid_pages_count.value)) return;

            const query = searchQuery.value && searchQuery.value.length > 0 ? `query=${searchQuery.value}&` : '';

            initLoading.value = true;

            try {
                const result = await sendRequest('GET', `/api/parcels?${query}page=${pageNumber}&count=${grid_page_size}`, undefined, store.getters.getAuthInfo?.userToken);
                
                parcelsList.value = result.list;
                grid_pages_count.value = result.pagesCount;
                grid_current_page.value = pageNumber;
               
            } catch (errorObj) {
                messages.value = requestErrorHandling(errorObj, router);
            } finally { initLoading.value = false; }


        }

        function openReport(parcelId) {
            const url = new URL(location.origin + `/reports/parcelpackets.html?requestAPI=${baseAPI}/api/parcels/${parcelId}`);
            window.open(url.toString(), '_blank');
        }


        return {
            parcelsList,
            getList,
            grid_pages_count,
            grid_current_page,
            grid_page_size,
            messages,
            searchQuery,
            store,
            initLoading,
            openReport
        };
    }


}
</script>

<style scoped></style>