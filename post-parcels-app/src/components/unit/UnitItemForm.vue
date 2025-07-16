<template>
   <div class="alert alert-secondary form-title-container" role="alert">
       <div class="d-flex justify-content-between">
          <div>
            <span v-if="editId"> 
             
              <span v-if="parentTitle">
               
                 {{ parentTitle }}
                  /
                  ویرایش زیر مرکز
               
               
              </span>
              <span v-else>
                  ویرایش مرکز
              </span>
            </span>
            <span v-else>
              ایجاد
                <span v-if="needToParent">
                  زیر مرکز 
                </span>
                <span v-else>
                  مرکز
                </span>
              جدید
            </span>
          </div>
          <div>
             <router-link v-if="parentTitle" :to="{name: 'unitList', query: {parentTitle}, params: {parentId: state.parentId}}">
                فهرست زیر مراکز
                {{ parentTitle }}
             </router-link>
             <router-link v-else :to="{name :'unitList'}">
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
          <button type="button" class="btn-close" aria-label="Close"  @click="clearMessages"></button>
     </div>
      <form @submit.prevent="submit">
          <div class="row">
            <div class="form-group col">
               <div class="form-check">
                <input class="form-check-input" type="checkbox" value="" id="flexCheckChecked" v-model="needToParent">
                <label class="form-check-label" for="flexCheckChecked">
                  زیر مجموعه یک مرکز است
                </label>
              </div>
            </div>
          </div>
          <div v-show="needToParent" class="row">
            <div class="form-group col">
             <select class="form-select"  :disabled="parentUnits.length === 0" v-model="state.parentId">
                <option v-if="parentUnits.length === 0" value="0"> گزینه‌ای برای انتخاب وجود ندارد </option>
                <option v-else :value="null"> ---- </option>
                <option v-for="parent of parentUnits" :key="parent.id" :value="parent.id"> {{ parent.title }} </option>
             </select>
              <div v-if="!parentIsValid" class="form-text text-danger rtl input-validation-text" > مرکزی انتخاب نشده است. </div>
            </div>
          </div>
          <div class="row">
            <div class="col-md-6">
              <label for="inputCode">کد </label>
              <input type="text" class="form-control ltr" id="inputCode" v-model="state.code" />
              <div v-if="v$.code.$error" class="form-text text-danger rtl input-validation-text"> کد را وارد کنید. </div>
            </div>
            <div class="col-md-6">
              <label for="inputTitle">عنوان </label>
              <input type="text" class="form-control rtl" id="inputTitle" v-model="state.title" />
              <div v-if="v$.title.$error" class="form-text text-danger rtl input-validation-text"> عنوان را وارد کنید. </div>
            </div>
          </div>
         
          <div style="text-align: end;">
            <button type="submit"  class="btn btn-primary mt-4" :disabled="formIsLoading">   
                <div v-if="formIsLoading">
                  <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"  >
               
                  </span>
                  صبر کنید ...
                </div>
                <span  v-else> ثبت </span>
                
            </button>
          </div>
         
      </form>
    </div>
</template>

<script>
    import { computed, reactive, onMounted, ref, watch} from 'vue';
    import { useRoute, useRouter } from 'vue-router';
    import { useStore } from 'vuex';
    import { useVuelidate } from '@vuelidate/core';
    import { required } from '@vuelidate/validators';
    import { sendRequest, showToast, requestErrorHandling } from './../../utility';

    export default {
      name: 'UnitItemForm',
      props: ['editId'],
    
      setup(props) {

        const route = useRoute();
        const router = useRouter();

        const editId = ref(props.editId);
        const parentTitle = ref(route.query.parentTitle);

        const store = useStore();
        let formIsLoading = ref(false);
        let messages = ref([]);

        let initLoading = ref(true);
        let needToParent = ref(false);
        let parentIsValid = ref(true);
        const parentUnits = ref([]);

        const state = reactive({
          title: null,
          code: null,
          active: true,
          parentId: null
        });

        const rules = computed(() => ({
          title: {
            required
          },
          code: {
            required
          },
        }));

        const v$ = useVuelidate(rules, state);

        onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.
        
          await getParentsList();
    
          if (editId.value) {
            await getEditItem(editId.value);
          } 
          
          initLoading.value = false;

        });

        async function add() {

          const formIsValid = await v$.value.$validate();
          parentIsValid.value = !(needToParent.value && !state.parentId);
        
          if (formIsValid && parentIsValid.value) {

            const params = {
              name: state.title, 
              code: state.code,
              parentId: needToParent.value ? state.parentId : null,
              active: state.active
            };

            formIsLoading.value = true;

            try {
              const result = await sendRequest('POST', '/api/units/add', params, store.getters.getAuthInfo?.userToken);

              if (needToParent.value === false) {
                parentUnits.value.push({ 
                  id: result.id, 
                  title: generateParentUnitTitle(result.code, result.name) 
                });
              }
              
              resetForm();

              showToast('success', 'مرکز جدید با موفقیت ثبت شد.');
            } catch (error) {
              messages.value = requestErrorHandling(error, router);
            } finally {
              formIsLoading.value = false;
            }     
   
          }

        }

        async function update() {
          const formIsValid = await v$.value.$validate();
          parentIsValid.value = !(needToParent.value && !state.parentId);

          if (formIsValid && parentIsValid.value) {

            const params = {
              name: state.title,
              code: state.code,
              parentId: needToParent.value ? state.parentId : null,
              active: state.active
            };
            formIsLoading.value = true;

            try {
              await sendRequest('PATCH', `/api/units/${editId.value}/update`, params, store.getters.getAuthInfo?.userToken);
             
              showToast('success', 'ویرایش مرکز با موفقیت انجام شد.');
              router.replace({name: 'unitList'}); 
            } catch (error) {
              messages.value = requestErrorHandling(error, router);
            } finally {
              formIsLoading.value = false;
            }

          }
        }

        async function submit() {
          clearMessages();
          const func = editId.value ? update : add;

          await func();
        }

        async function getParentsList() {

          formIsLoading.value = true;

          try {
            const result = await sendRequest('GET', `/api/units?all=true&onlyParents=true`, undefined, store.getters.getAuthInfo?.userToken);
            
            if (result.list && result.list.length > 0) {
              result.list.forEach(element => {
                parentUnits.value.push({ 
                  id: element.id,
                  title: generateParentUnitTitle(element.code, element.name)
                });
              });
              state.parentId = parentUnits.value[0].id;
            }
          } catch (error) {
            messages.value = requestErrorHandling(error, router);
          } finally { formIsLoading.value = false; }


        }

        function generateParentUnitTitle(code, name) {
          return `${name} (${code})`;
        }

        async function getEditItem(editIdValue) {
          formIsLoading.value = true;

          try {
            const result = await sendRequest('GET', `/api/units/${editIdValue}`, undefined, store.getters.getAuthInfo?.userToken);

            state.parentId = !result.parentId ? null : result.parentId;
            state.code = result.code;
            state.title = result.name;
            needToParent.value = result.parentId !== null;       

          
          } catch (error) {
            messages.value = requestErrorHandling(error, router);
          } finally { formIsLoading.value = false; }
        }

        function resetForm() {
          state.code = null;
          state.title = null;
        }

        function clearMessages() {
           messages.value = [];
        }

        watch([route], () => {
          parentTitle.value = route.query.parentTitle
        });

        return {
          state, 
          formIsLoading, 
          messages, 
          v$,
          needToParent,
          parentUnits,
          parentIsValid,
          submit,
          clearMessages,
          initLoading,
          parentTitle
        }
      }
    }
</script>

<style scoped>

</style>