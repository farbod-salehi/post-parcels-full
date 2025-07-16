<template>
   
   <div id="header">
      <div v-if="userIsAuthenticated">
           <div id="menu-icon" >
               منو
           </div>  
      </div>
      <div style="font-weight: bold; font-size:1.2em; margin:0px auto">
       مدیریت مرسوله‌های پستی
      </div>
      <div style="margin-inline-end:15px" v-if="userIsAuthenticated">
          <ul class="navbar-nav">
            <li class="nav-item dropdown">
              <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                {{ userAuthInfo.userTitle }}
              </a>
              <ul class="dropdown-menu">
                <!-- <li><a class="dropdown-item" href="#">حساب کاربری </a></li>-->
                <li> <RouterLink :to="{name: 'changeMyPassword'}" class="dropdown-item"> تغییر کلمه عبور  </RouterLink> </li>
                <li><hr class="dropdown-divider"></li>
                <li><a class="dropdown-item" style="display:inline-block; cursor: pointer;" data-bs-toggle="modal" data-bs-target="#signoutModal" >خروج</a></li>
              </ul>
            </li>
          </ul>
      </div>
      
   </div>
    <div id="main">
            <div id="menu-main" v-if="userIsAuthenticated">
                  <div class="mt-4">
                   
                      <RouterLink :to="{ name: 'parcelList' }">
                         <b-icon-envelope-fill style="font-size: 1.3em;" />
                          &nbsp;
                         پکیج‌ها
                        </RouterLink>
                   
                 </div>
                 <div class="mt-4">
                    <RouterLink :to="{ name: 'parcelItemsTracking' }">
                      
                        <b-icon-search style="font-size: 1.3em;" />
                         &nbsp;
                       ردگیری مرسوله‌ها
                      
                      </RouterLink>
                    
                  </div>
                  <div class="mt-4">
                    <RouterLink :to="{ name: 'parcelStatistic' }">
                      
                        <b-icon-bar-chart-line-fill style="font-size: 1.3em;" />
                         &nbsp;
                        آمار پکیج‌ها
                      
                      </RouterLink>
                    
                  </div>
                 <div class="mt-4" >                     
                    <RouterLink :to="{name: userRole !== 1 && userRole !== 2 ?  undefined : 'unitList'}" :class="userRole !== 1 &&  userRole !== 2 ? 'disabled' : undefined">
                       <b-icon-building-fill style="font-size: 1.3em;"  />
                       &nbsp;
                      مراکز
                      (گیرنده/فرستنده)
                     </RouterLink>
                 </div>
                   <div class="mt-4">
                        <RouterLink :to="{ name: userRole !== 1 ? undefined : 'userList' }" :class="userRole !== 1 ? 'disabled' : undefined">
                      
                            <b-icon-people-fill style="font-size: 1.3em;" />
                             &nbsp;
                             کاربران
                          </RouterLink>
                    
                   </div>
                 
                 
            </div>
            <div id="container-main">
              <div class="card card-body shadow-sm" id="content">
                 <router-view> </router-view>
              </div>
            </div>
    </div>
    <div id="footer">
        طراحی و توسعه:
        <br />
          معاونت فناوری اطلاعات و برنامه‌ریزی دادگستری استان آذربایجان غربی
          -
          نسخه ({{ version }})
    </div>
     <div  class="modal fade" id="signoutModal" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="signoutModalLabel" aria-hidden="true">
              <div class=" modal-dialog modal-dialog-centered modal-dialog-scrollable">
                  <div class="modal-content">
                      <div class="modal-header">
                          <h4 class="modal-title fs-5" id="signoutModalLabel"> خروج از حساب کاربری</h4>
                          <button  type="button"  class="btn-close" data-bs-dismiss="modal" aria-label="Close" @click="closeSignoutModal">
                
                          </button>
                      </div>
                      <div class="modal-body">
                          آیا می‌خواهید از حساب کاربری خود خارج شوید؟
                      </div>
                      <div class="modal-footer">
                              <button class="btn btn-danger btn-sm" data-bs-dismiss="modal">
                              خیر
                          </button>
                          &nbsp;
                          <button class="btn btn-success btn-sm"  data-bs-dismiss="modal" @click="signOut">
                              بله
                          </button>
                    
                          </div>
                  </div>
              </div>
      </div>
   
  
</template>

<script>
  import { computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { useStore } from 'vuex';
  import {sendRequest, showToast} from "./utility";

  export default {
    name: 'App',
    setup() {

      const store = useStore();
      const router = useRouter();

      store.commit('tryLogin');
      
      let version = "2.3";

      if(store.getters.userIsAuthenticated === false) {
        clearAuthInfo();
      } 
     
      function clearAuthInfo() {
        store.commit('removeAuthInfo');
        router.replace({name: 'login'});
      }
      
      async function signOut() {

        //initLoading.value = true;

        try {
            
           await sendRequest('POST','/api/signout', null, store.getters.getAuthInfo?.userToken);
           clearAuthInfo();
        } catch (error) {
            showToast('error', 'اجرای درخواست با خطا مواجه شده است.');
        } finally { /*initLoading.value = false;*/ }

       
       
      }

      return {
        userIsAuthenticated: computed(() => store.getters.userIsAuthenticated) ,
        userAuthInfo: computed(() => store.getters.getAuthInfo),
        signOut,
        userRole: computed(() => store.getters.getAuthInfo?.userRole),
        version
      };
    }
  }

</script>

<style>
  
</style>
