<template>
     <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
              <div>
                
                <span v-if="parentIdRef">
                      <span v-if="parentTitle">
                            
                            {{ parentTitle }}
                             /
                    </span>
                    فهرست زیر مراکز
                  
                </span>
                <span v-else>
                    فهرست مراکز
                </span>
              </div>
              <div>
                 <router-link v-if="!parentIdRef" :to="{ name: 'unitItem' }">
                   ایجاد مرکز جدید
                 </router-link>
                 <router-link v-if="parentIdRef" :to="{ name: 'unitList' }">
                      فهرست مراکز
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
                    <input v-model="searchQuery" @keyup.enter="getList(1)" type="text" class="form-control rtl" style="display: inline; width:60%;"
                        id="searchQuery" placeholder="جستجو با عنوان" />
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
                            <th scope="col">کد</th>
                            <th scope="col"> عنوان</th>
                            <th scope="col">وضعیت</th>
                    
                            <th scope="col"></th>
                        </tr>
                    </thead>
                    <tbody style="text-align:center; font-size: 0.9em;">
                        <tr v-for="(unit,index) of unitsList" :key="unit.id">
                            <th scope="row">{{ ((grid_current_page - 1) * Number(grid_page_size)) + (index + 1) }}</th>

                            <td>
                                {{ unit.code }}
                            </td>
                            <td>
                                {{ unit.name }}
                            </td>
                            <td>
                                {{ unit.active ? 'فعال' : 'غیرفعال' }}
                            </td>
                       
                            <td>
                                <router-link :to="{ name: 'unitItem', query: { parentTitle: parentIdRef ? parentTitle : undefined }, params: { editId: unit.id } }">
                                    ویرایش
                                </router-link>
                                <span v-if="!parentIdRef"> 
                                    &nbsp;
                                    |
                                    &nbsp;
                                </span>
                                 <router-link v-if="!parentIdRef" :to="{ name: 'unitList', query: {parentTitle: unit.name}, params: { parentId: unit.id } }">
                                    زیر مراکز
                                </router-link>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div v-if="grid_pages_count === 0 && initLoading === false">
                    <div class="alert alert-info sm" role="alert">
                        داده‌ای برای نمایش یافت نشد.
                    </div>
                </div>
                <div v-if="grid_pages_count > 1" style="text-align: center; margin-top:40px;">

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
import { ref, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useStore } from 'vuex';
import { sendRequest, requestErrorHandling } from './../../utility';

export default {
    name: 'UnitListForm',
    props: ['parentId'],
    setup(props) {

        const route = useRoute();
        const router = useRouter();

        const parentIdRef = ref(props.parentId);
        let parentTitle = ref(route.query.parentTitle);
        const store = useStore();

        let messages = ref([]);

        let initLoading = ref(false);
        let unitsList = ref([]);
        const grid_page_size = 15;
        let grid_pages_count = ref(1);
        let grid_current_page = ref(1);
        let searchQuery = ref('');
       

        onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.
            //const result = await sendRequest('GET', `/units/import`, undefined, store.getters.getAuthInfo?.userToken);
            //console.log(result);
            await getList(1);
        });

        async function getList(pageNumber) {
            if (pageNumber <= 0 || (grid_pages_count.value !== 0 && pageNumber > grid_pages_count.value)) return;

            const query = (parentIdRef.value ? `parentId=${parentIdRef.value}&` : '') + (searchQuery.value && searchQuery.value.length > 0 ? `name=${searchQuery.value}&` : '');

            initLoading.value = true;

            try {
                const result = await sendRequest('GET', `/api/units?${query}page=${pageNumber}&count=${grid_page_size}`, undefined, store.getters.getAuthInfo?.userToken);
               
                
                unitsList.value = result.list;
                grid_pages_count.value = result.pagesCount;

                grid_current_page.value = pageNumber;

                

            } catch (error) {
                messages.value = requestErrorHandling(error, router);
            } finally { initLoading.value = false; }


        }


        watch([props], async(newValues) => {
            parentIdRef.value = newValues[0].parentId;
            parentTitle.value = route.query.parentTitle;
          
           
            grid_current_page.value = 1;
            searchQuery.value= '';

            initLoading.value = true;
            await getList(1);
            initLoading.value = false;
        });

        
        return {
            unitsList,
            getList,
            grid_pages_count,
            grid_current_page,
            grid_page_size,
            messages,
            searchQuery,
            store,
            initLoading,
            parentTitle,
            parentIdRef,
        };
    }


}
</script>

<style scoped></style>