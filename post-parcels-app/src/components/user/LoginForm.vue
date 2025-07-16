<template>
         <div class="alert alert-secondary form-title-container" role="alert">
             <div class="d-flex justify-content-between">
                <div>
                  ورود به حساب کاربری
                </div>
                <div>
                
                  
                </div>
             </div>
        
          </div>
          <div id="form-container">
             <div v-show="messages.length > 0" class="alert alert-danger alert-dismissible fade show" style="margin-bottom:50px" role="alert">
                  <div v-for="mes of messages" :key="mes" class="mt-1">
                      - {{ mes }}
                  </div>
                  <button type="button" class="btn-close" aria-label="Close"  @click="clearMessages"></button>
              </div>
              <form @submit.prevent = "login">
                <div class="row">
                   <div class="col">
                        <label for="inputUsername"> نام کاربری </label>
                        <input type="text" class="form-control ltr" id="inputUsername" v-model="state.username" />
                        <div v-if="v$.username.$error" class="form-text text-danger rtl input-validation-text">
                             نام کاربری را وارد نمایید.
                        </div>
                    
                    </div>
                </div>
                <div class="row mt-2">
                     <div class="col">
                          <label for="inputPassword">  کلمه عبور</label>
                          <input type="password" class="form-control ltr" id="inputPassword" v-model="state.password" />
                          <div v-if="v$.password.$error" class="form-text text-danger rtl input-validation-text">
                              کلمه عبور را وارد نمایید.
                          </div>
                    
                      </div>
                </div>
                 <div class="row" style="text-align: end;">
                   <div class="col">
                      <button type="submit"  class="btn btn-primary mt-4" :disabled="formIsLoading">   
                          <div v-if="formIsLoading">
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"  >
                    
                            </span>
                            صبر کنید ...
                          </div>
                          <span  v-else> ورود </span>
                      
                      </button>
                   </div>
                </div>
              </form>
          </div>
    
     
</template>

<script>
import { reactive, computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useVuelidate} from '@vuelidate/core';
import { required } from '@vuelidate/validators';
import { useStore } from 'vuex';
import { sendRequest, requestErrorHandling} from './../../utility';

export default {
  name: 'LoginForm',
  setup() {

    const router = useRouter();
    const store = useStore();

    let formIsLoading = ref(false);
    let messages = ref([]);

    const state = reactive({
      username: null,
      password: null,
    });

    const rules = computed(() => ({
      username: {
        required
      },
      password: {
        required
      }
    }));

    const v$ = useVuelidate(rules, state);

    async function login() {
      messages.value = [];

      const formIsValid = await v$.value.$validate();
  
      if (formIsValid) {

        const params = {
          username: state.username, 
          password: state.password
        };

        formIsLoading.value = true;

        try {
         
          const result = await sendRequest('POST', '/api/login', params);
          
          store.commit('setAuthInfo', { userToken: result.userToken, userTitle: result.userTitle, userRole: result.userRole });
      
          router.replace( {name: 'parcelItem'});

        } catch (errorObj) {   
              messages.value = requestErrorHandling(errorObj, router);
        } finally {
          formIsLoading.value = false;
        }        
      }
    }

    function clearMessages() {
      messages.value = [];
    }

    return { 
      state, 
      v$, 
      login, 
      formIsLoading, 
      messages, 
      clearMessages
    };

  }
}
</script>

<style scoped>
     #form-container {
      width:90%;
      margin:0px auto;
     }
    @media screen and (min-width:768px) {
      #form-container {
        max-width:50%;    
      }
    }
  
</style>