<template>
     <div class="alert alert-secondary form-title-container" role="alert">
          <div class="d-flex justify-content-between">
            <div>
              ویرایش حساب کاربری 
              {{ state.username }}
            </div>
            <div>
            
                <router-link :to="{ name: 'userList' }">
                فهرست کاربران
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
           <form @submit.prevent="save">
              <div class="row">
                <div class="col-md-6">
                  <label for="inputState">نوع کاربری</label>
                  <select id="type" class="form-control" v-model="state.type">
                    <option  v-for="ut of user_types" :key="ut.value" :value="ut.value"> {{ ut.name }} </option>
                  </select>
                  <small v-if="v$.type.$error" class="form-text text-danger rtl"> نوع کاربری را مشخص کنید. </small>
                </div>
                <div class="col-md-6">
                    <label for="title"> عنوان کاربر </label>
                  <input type="text" class="form-control rtl" id="title" v-model="state.title">
                  <small v-if="v$.title.$error" class="form-text text-danger rtl"> عنوان را وارد نمایید. </small>
                </div>
              </div>
         
              <div class="row mt-4">
                <div class="col">
                   <div class="form-check">
                      <input class="form-check-input" type="checkbox" value="" id="flexCheckChecked" v-model="passwordWillChange">
                      <label class="form-check-label" for="flexCheckChecked">
                       ثبت کلمه عبور جدید
                      </label>
                   </div>
                </div>
              </div>
              <div  class="row mt-2">
                <div class="col-md-6">
                   <label for="password">کلمه عبور</label>
                    <input type="password" class="form-control ltr" id="password" v-model="state.password" :disabled="!passwordWillChange">
                    <small v-if="v$.password.$error" class="form-text text-danger rtl">
                        حداقل طول برای کلمه عبور
                        {{ password_minLength }}
                        است.
                    </small>
                </div>
                <div class="col-md-6">
                    <label for="passwordConfirm"> تکرار کلمه عبور </label>
                    <input type="password" class="form-control ltr" id="passwordConfirm" v-model="state.passwordConfirm" :disabled="!passwordWillChange">
                    <small v-if="v$.passwordConfirm.$error" class="form-text text-danger rtl"> کلمه عبور و تکرار آن باید یکسان باشند. </small>
                </div>
              </div>
          
              <div class="form-group" style="text-align: end;">
                <button type="submit"  class="btn btn-primary mt-4" @click="save" :disabled="formIsLoading">   
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
    import { ref, reactive, computed, onMounted} from 'vue';
    import { useStore } from 'vuex';
    import { useRouter } from 'vue-router';
    import { useVuelidate } from '@vuelidate/core';
    import { required } from '@vuelidate/validators';
    import { sendRequest, showToast, requestErrorHandling } from './../../utility';
  
    export default {
        name: 'UserEditForm',
        props: ['id'],
        setup(props) {

            const router = useRouter();
            const store = useStore();

            const password_minLength = 6;
            const user_types = [{name: 'عادی' , value : 3}, {name: 'مدیر', value: 2}];

            let formIsLoading = ref(false);
            let messages = ref([]);
            let passwordWillChange = ref(false);
            let initLoading = ref(true);

            const editId = ref(props.id);

            const state = reactive({
              username: null,
              password: null,
              passwordConfirm: null,
              title: null,
              type: user_types[0].value,
            });

            const rules = computed(() => ({
              username: {
                required
              },
              password: {
                requiredIf: () => passwordWillChange.value ? state.password?.length >= password_minLength : true
              },
              passwordConfirm: {
               requiredIf: () => passwordWillChange.value ?  state.password === state.passwordConfirm : true
              },
              title: {
                required
              },
              type: {
                required,
              }
            }));

            const v$ = useVuelidate(rules, state);

            onMounted(async () => { // Access to template ref and using await/async in root of 'setup()' will be possible in 'onMounted'.

              if (editId.value) {
                await getUser();
              }

              initLoading.value = false;

            });

            async function getUser() {

               try {
                const result = await sendRequest('GET', `/api/users/${editId.value}`, undefined, store.getters.getAuthInfo?.userToken);
                
                state.username = result.userName;
                state.title = result.title;
                state.type = result.role;

              } catch (error) {
                messages.value = requestErrorHandling(error, router);
              } finally {
                formIsLoading.value = false;
              }
            }

            async function save() {
              messages.value = [];
              const formIsValid = await v$.value.$validate();
              if(formIsValid) {
                  const params = {
                    password: passwordWillChange.value ? state.password : null,
                    title: state.title,
                    role: state.type
                  };

                 
                  formIsLoading.value = true;

                  try {
                    await sendRequest('PATCH', `/api/users/${editId.value}/update`, params, store.getters.getAuthInfo?.userToken);
                   
                    showToast('success', `ویرایش حساب کاربری ${state.username} با موفقیت انجام شد.`);

                    router.replace({ name: 'userList' });

                  } catch (error) {
                    messages.value = requestErrorHandling(error, router);
                  } finally {
                    formIsLoading.value = false;
                  }     
              }
            }

            function clearMessages() {
              messages.value = [];
            }
            
            return {
              formIsLoading, 
              messages, 
              state, 
              password_minLength, 
              passwordWillChange,
              user_types, 
              v$,
              initLoading,
              save,
              clearMessages
            };
        }
    }
</script>

<style scoped>

</style>