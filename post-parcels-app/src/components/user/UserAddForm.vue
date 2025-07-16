<template>
    <div class="alert alert-secondary form-title-container" role="alert">
        <div class="d-flex justify-content-between">
          <div>
            ایجاد حساب کاربری 
          </div>
          <div>
            
              <router-link :to="{ name: 'userList' }">
              فهرست کاربران
              </router-link>
          </div>
        </div>
      
    </div>
    <div>
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
                
              </div>
            </div>
         
            <div class="row mt-3">
               <div class="col-md-6">
                   <label for="title"> عنوان کاربر </label>
                  <input type="text" class="form-control rtl" id="title" v-model="state.title">
                  <small v-if="v$.title.$error" class="form-text text-danger rtl"> عنوان را وارد نمایید. </small>
                </div>
               <div class="col-md-6">
                  <label for="username">نام کاربری</label>
                  <input type="text" class="form-control ltr" id="username" v-model="state.username">
                  <small v-if="v$.username.$error" class="form-text text-danger rtl"> نام کاربری را وارد نمایید. </small>
               </div>
            </div>
            <div class="row mt-3">
              <div class="col-md-6">
                 <label for="password">کلمه عبور</label>
                  <input type="password" class="form-control ltr" id="password" v-model="state.password">
                  <small v-if="v$.password.$error" class="form-text text-danger rtl">
                      حداقل طول برای کلمه عبور
                      {{ password_minLength }}
                      است.
                  </small>
              </div>
              <div class="col-md-6">
                  <label for="passwordConfirm"> تکرار کلمه عبور </label>
                  <input type="password" class="form-control ltr" id="passwordConfirm" v-model="state.passwordConfirm">
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
    import { ref, reactive, computed} from 'vue';
    import {useStore} from 'vuex';
    import {useRouter} from 'vue-router'
    import { useVuelidate } from '@vuelidate/core';
    import { required, minLength } from '@vuelidate/validators';
    import { sendRequest, showToast, requestErrorHandling} from './../../utility';
  
    export default {
        name: 'UserAddForm',
        setup() {
            
            const store = useStore();

            const router = useRouter();
            const password_minLength = 6;
           

            const user_types = [{name: 'عادی' , value : 3}, {name: 'مدیر', value: 2}];
            let formIsLoading = ref(false);
            let messages = ref([]);

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
                required,
                minLength: minLength(password_minLength)
              },
              passwordConfirm: {
                required,
                minLength: minLength(password_minLength)
              },
              title: {
                required
              },
              type: {
                required,
              }
            }));
            
            const v$ = useVuelidate(rules, state);

            async function save() {

              messages.value = [];
              const formIsValid = await v$.value.$validate();

              if(formIsValid) {

               const params = {
                  username: state.username,
                  password: state.password,
                  title: state.title,
                  role: state.type
                };

                formIsLoading.value = true;

                try {
                  await sendRequest('POST', '/api/users/add', params, store.getters.getAuthInfo?.userToken);
                 
                  state.username = '';
                  state.password = '';
                  state.passwordConfirm = '';
                  state.title = '';

                  showToast('success', 'حساب کاربری جدید ایجاد شد.');

                  router.replace({ name: 'userList' });

                } catch (error) {
                  messages.value = requestErrorHandling(error, router);
                } finally {
                  formIsLoading.value = false;
                }

                     
              }
            }

            return {
              formIsLoading, 
              messages, 
              state, 
              password_minLength, 
              user_types, 
              v$, 
              save
            };
        }
    }
</script>

<style scoped>

</style>