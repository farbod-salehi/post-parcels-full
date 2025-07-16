<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
            <div>
                ردگیری مرسوله‌ها
            </div>
            <div>
                
            </div>
        </div>
    </div>
    <!--<div v-if="initLoading" style="text-align: center">
        <div class="spinner-border text-secondary" role="status">
        <span class="visually-hidden">Loading...</span>
        </div>
    </div>-->
    <div>

        <div v-show="messages.length > 0" class="alert alert-danger alert-dismissible fade show" role="alert">
            <div v-for="mes of messages" :key="mes" class="mt-1">
                - {{ mes }}
            </div>
            <button type="button" class="btn-close" aria-label="Close" @click="clearMessages"></button>
        </div>

        <div style="margin-top: 30px; margin-bottom: 50px;">
            <input v-model="searchQuery" @keyup.enter="getList(1)" type="text" class="form-control rtl"
                style="display: inline; width:60%;" id="searchQuery" placeholder="جستجو با شناسه مرسوله" />
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
                    <th scope="col">شناسه مرسوله</th>
                    <th scope="col"> نوع</th>
                    <th scope="col"> کد پکیج</th>
                    <th scope="col"> فرستنده</th>
                    <th scope="col"> گیرنده</th>   
                    <th scope="col">تاریخ ثبت</th>

                   
                </tr>
            </thead>
            <tbody style="text-align:center; font-size: 0.8em;">
                <tr v-for="(parcelItem,index) of parcelItemsList" :key="parcelItem.id">
                    <th scope="row">
                        {{ ((grid_current_page - 1) * Number(grid_page_size)) + (index + 1) }}
                    
                    </th>

                    <td>
                        {{ parcelItem.code }}
                    </td>
                    <td>                   
                        {{ parcelItem.type }}
                    </td>
                    <td>                 
                       
                        <router-link style="border-bottom:1px solid"
                            :to="{ name: 'parcelItem',params: { editId: parcelItem.parcelId } }">
                            {{ parcelItem.parcelCode }}
                        </router-link> 
                    </td>
                    <td>                 
                        {{ parcelItem.sender }}
                    </td>
                    <td>                 
                        {{ parcelItem.receiver }}
                    </td>
                    <td style="direction: ltr;">
                        {{ parcelItem.createdAt }}
                    </td>

                   
                </tr>
            </tbody>
        </table>
        <div v-if="grid_pages_count === 0">
            <div v-if="initLoading === true" class="alert alert-info sm" role="alert">
             یک شناسه مرسوله را جستجو کنید.
            </div>
            <div v-else class="alert alert-info sm" role="alert">
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
import { sendRequest, requestErrorHandling } from './../../utility';

export default {
    name: 'ParcelItemTrackingForm',
    setup() {

        const router = useRouter();

        const store = useStore();

        let messages = ref([]);

        let initLoading = ref(true);
        let parcelItemsList = ref([]);
        const grid_page_size = 10;
        let grid_pages_count = ref(0);
        let grid_current_page = ref(0);
        let searchQuery = ref('');


        onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.
            //await getList(1);
        });

        async function getList(pageNumber) {
           
            if (pageNumber <= 0 || (grid_pages_count.value !== 0 && pageNumber > grid_pages_count.value)) return;
            
            let query = '';
           
            if(searchQuery.value && searchQuery.value.length > 0) {
                
                initLoading.value = false; 
                query = `query=${searchQuery.value}&`;

                try {
                    const result = await sendRequest('GET', `/api/parcels/items?${query}page=${pageNumber}&count=${grid_page_size}`, undefined, store.getters.getAuthInfo?.userToken);
                    
                    parcelItemsList.value = result.list;
                    grid_pages_count.value = result.pagesCount;
                    grid_current_page.value = pageNumber;
                
                } catch (errorObj) {
                    messages.value = requestErrorHandling(errorObj, router);
                } finally { /*initLoading.value = false;*/ }

            } else {
                parcelItemsList.value = [];
                grid_pages_count.value = 0;
                grid_current_page.value = 0;
                initLoading.value = true; 
            }

        }

        
        return {
            parcelItemsList,
            getList,
            grid_pages_count,
            grid_current_page,
            grid_page_size,
            messages,
            searchQuery,
            store,
            initLoading,
        };
    }
}
</script>

<style scoped>

</style>