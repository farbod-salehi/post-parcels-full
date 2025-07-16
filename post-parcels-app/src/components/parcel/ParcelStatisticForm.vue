<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
            <div>
               آمار پکیج‌ها
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
        <div  class="alert alert-warning fade show" role="alert">
           
            <ul>
                <li>
                    از ۱۴۰۴/۰۱/۱۴
                </li>
                <li>
                    از ۱۴۰۴/۰۱/۱۴
                    الی ۱۴۰۴/۰۴/۰۵
                </li>
                <li>
                    
                    الی ۱۴۰۴/۰۴/۰۵
                </li>
            </ul>
          
        </div>
        <div v-show="messages.length > 0" class="alert alert-danger alert-dismissible fade show" role="alert">
            <div v-for="mes of messages" :key="mes" class="mt-1">
                - {{ mes }}
            </div>
            <button type="button" class="btn-close" aria-label="Close" @click="clearMessages"></button>
        </div>

        <div style="margin-top: 30px; margin-bottom: 50px;">
            از
            &nbsp;
            <input v-model="from" @keyup.enter="getList(1)" type="text" class="form-control ltr" autocomplete="off"
                style="display: inline; width:120px;" id="fromQuery" placeholder="" />
            &nbsp;
            الی
            &nbsp;
            <input v-model="to" @keyup.enter="getList(1)" type="text" class="form-control ltr" autocomplete="off"
                style="display: inline; width:120px;" id="toQuery" placeholder="" />
            &nbsp;
            <button class="btn btn-primary btn-sm" @click="getList(1)">
              
                <div v-if="formIsLoading">
                    <span
                    class="spinner-border spinner-border-sm"
                    role="status"
                    aria-hidden="true"
                    >
                    </span>
                    صبر کنید ...
                </div>
                <span v-else>   
                    <b-icon-search />
                    جستجو
                </span>
            </button>
        </div>
        <table v-if="grid_pages_count > 0" class="table table-sm table-striped" style="width:100%">
            <thead style="text-align:center">
                <tr>
                    <th scope="col"> ردیف </th>
                    <th scope="col"> تاریخ </th>
                    <th scope="col"> تعداد</th>
                </tr>
            </thead>
            <tbody style="text-align:center; font-size: 0.8em;">
                <tr v-for="(item,index) of itemsList" :key="item.id">
                    <th scope="row">
                        {{ ((grid_current_page - 1) * Number(grid_page_size)) + (index + 1) }}
                    
                    </th>

                    <td>
                        {{ item.date }}
                    </td>
                    <td>                   
                        {{ item.count }}
                    </td>
                  
                </tr>
            </tbody>
        </table>
        <div v-if="grid_pages_count === 0">
            <div v-if="initLoading === true" class="alert alert-info sm" role="alert">
             یک بازه زمانی را جستجو کنید.
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
        name: 'ParcelStatisticForm',
        setup() {

            const router = useRouter();

            const store = useStore();

            let messages = ref([]);
            
            let formIsLoading = ref(false);
            let initLoading = ref(true);
            let itemsList = ref([]);
            const grid_page_size = 10;
            let grid_pages_count = ref(0);
            let grid_current_page = ref(0);
            let from = ref('');
            let to = ref('');


            onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.
                //await getList(1);
            });

            async function getList(pageNumber) {
            
                if (pageNumber <= 0 || (grid_pages_count.value !== 0 && pageNumber > grid_pages_count.value)) return;
                
                let query = '';
                let hasFrom = from.value && from.value.replaceAll('/','').length > 0;
                let hasTo = to.value && to.value.replaceAll('/','').length > 0;

                if(hasFrom || hasTo) {
                    
                    initLoading.value = false; 
                    
                    formIsLoading.value = true;

                    if (hasFrom) {
                        query += `from=${from.value.replaceAll('/','')}&`;
                    }
                  
                    if (hasTo) {
                        query += `to=${to.value.replaceAll('/','')}&`;
                    }

                    try {
                        const result = await sendRequest('GET', `/api/parcels/statistic?${query}`, undefined, store.getters.getAuthInfo?.userToken);
                        
                        itemsList.value = result.list;
                        grid_pages_count.value = result.pagesCount;
                        grid_current_page.value = pageNumber;
                    
                    } catch (errorObj) {
                        messages.value = requestErrorHandling(errorObj, router);
                    } finally { formIsLoading.value = false; }

                } else {
                    itemsList.value = [];
                    grid_pages_count.value = 0;
                    grid_current_page.value = 0;
                    initLoading.value = true; 
                }

            


            }

            


            return {
                itemsList,
                getList,
                grid_pages_count,
                grid_current_page,
                grid_page_size,
                messages,
                from,
                to,
                store,
                initLoading,
                formIsLoading
            };
        }
    }
</script>

<style scoped>

</style>