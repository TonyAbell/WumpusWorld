using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using WumpusWorldWebApi.Models;
using System.Threading.Tasks;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Configuration;
using System.Security.Claims;

namespace WumpusWorldWebApi.EntityStore
{

    public class UserStore<TUser> : IUserLoginStore<TUser>,
                                    IUserClaimStore<TUser>,
                                    IUserRoleStore<TUser>,
                                    IUserPasswordStore<TUser>,
                                    IUserSecurityStampStore<TUser>,
                                    IUserStore<TUser>,
                                    IDisposable where TUser : User
    {
      
        public UserStore()
        {
            
        }
        
        public async Task AddLoginAsync(TUser user, UserLoginInfo login)
        {
            var entity = new UserLogin(user.Id, login.LoginProvider, login.ProviderKey);
            var op = TableOperation.Insert(entity);
            await Azure.userloginstoreTable.ExecuteAsync(op);                       
        }

        public async Task<TUser> FindAsync(UserLoginInfo login)
        {

            var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(login.ProviderKey));
            var op = TableOperation.Retrieve<UserLogin>(login.LoginProvider, rowKey);
            TableResult results = await Azure.userloginstoreTable.ExecuteAsync(op);
            if (results.Result == null)
            {
                return null;
            }
            else
            {
                var userLogin = results.Result as UserLogin;
                var userId = userLogin.UserId;
                op = TableOperation.Retrieve<User>("user", userId);
                results = await Azure.userstoreTable.ExecuteAsync(op);
                if (results.Result == null)
                {
                    return null;
                }
                else
                {
                    var user = results.Result as User;
                    return user as TUser;
                }
            }
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user)
        {

            var q = new TableQuery<UserLogin>().Where(string.Format("UserId eq '{0}'", user.Id));
            var tro = new TableRequestOptions();
            TableContinuationToken token = new TableContinuationToken();
           
            var r = await Azure.userloginstoreTable
                               .ExecuteQuerySegmentedAsync<UserLogin>(q,token);
                        
                        
            var returnValue = r.Select(s => 
            {
                return new UserLoginInfo(s.LoginProvider,s.ProviderKey);
            }).ToList();

            return returnValue;
            
        }

        public async Task RemoveLoginAsync(TUser user, UserLoginInfo login)
        {
            var q = new TableQuery<UserLogin>().Where(string.Format("UserId eq '{0}'", user.Id));
            var tro = new TableRequestOptions();
            TableContinuationToken token = new TableContinuationToken();

            var r = await Azure.userloginstoreTable
                               .ExecuteQuerySegmentedAsync<UserLogin>(q, token);


            var deleteEntity = r.Where(w => w.LoginProvider == login.LoginProvider && w.ProviderKey == login.ProviderKey).FirstOrDefault();
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                await Azure.userloginstoreTable.ExecuteAsync(deleteOperation);
            }

            

        }

        public async Task CreateAsync(TUser user)
        {
            Random r = new Random();
            var apiToken = r.Next(0, int.MaxValue).ToString();
            var entity = user as User;
            entity.ApiToken = apiToken;
            var op = TableOperation.InsertOrMerge(entity);
            var results = await Azure.userstoreTable.ExecuteAsync(op);

            if (results.Result == null)
            {
                return;
            }
            else
            {
                WumpusWorld.ApiToken tokenEntity = new WumpusWorld.ApiToken();
                tokenEntity.PartitionKey = WumpusWorld.ApiToken.PartitionKeyName;
                tokenEntity.RowKey = apiToken;

                tokenEntity.UserId = user.Id;
                tokenEntity.IsActive = true;

                var apiTokenOp = TableOperation.InsertOrMerge(tokenEntity);
                var insertTokenResult = await Azure.apitokensTable.ExecuteAsync(apiTokenOp);                
            }

        }

        public async Task DeleteAsync(TUser user)
        {
         
        }

        public async Task<TUser> FindByIdAsync(string userId)
        {
            var op = TableOperation.Retrieve<User>("user", userId);
            var results = await Azure.userstoreTable.ExecuteAsync(op);
            if (results.Result == null)
            {
                return null;

            }
            else
            {
                var user = results.Result as User;
                return user as TUser;
            }
        }

        public async Task<TUser> FindByNameAsync(string userName)
        {

            var q = new TableQuery<User>().Where(string.Format("UserName eq '{0}'", userName));
            var tro = new TableRequestOptions();
            TableContinuationToken token = new TableContinuationToken();

            var r = Azure.userstoreTable
                               .ExecuteQuery<User>(q);

            if (r.Count() == 0 )
            {
                return null;
            } else
            {
                var u = r.FirstOrDefault() as TUser;
                return u;
            }
           
           
            
        }

        public async Task UpdateAsync(TUser user)
        {
            var op = TableOperation.InsertOrMerge(user);
            var result = await Azure.userstoreTable.ExecuteAsync(op);  
        }

      

        public async Task AddClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
            
        }

        public async Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(TUser user)
        {
            return new List<Claim>();
        }

        public  async Task RemoveClaimAsync(TUser user, System.Security.Claims.Claim claim)
        {
           
        }

        public async Task AddToRoleAsync(TUser user, string role)
        {
           
        }

        public async Task<IList<string>> GetRolesAsync(TUser user)
        {
            return new List<string>();
        }

        public async Task<bool> IsInRoleAsync(TUser user, string role)
        {
            return false;
        }

        public async Task RemoveFromRoleAsync(TUser user, string role)
        {
            
        }

        public async Task<string> GetPasswordHashAsync(TUser user)
        {
            var op = TableOperation.Retrieve<User>("user", user.Id);
            var results = await Azure.userstoreTable.ExecuteAsync(op);
            if (results.Result == null)
            {
                return "";
            }
            else
            {
                return (results.Result as TUser).PasswordHash;
            }
        }

        public async Task<bool> HasPasswordAsync(TUser user)
        {
            var op = TableOperation.Retrieve<User>("user", user.Id);
            var results = await Azure.userstoreTable.ExecuteAsync(op);
            if (results.Result == null)
            {
                return false;
            }
            else
            {
                var u = results.Result as TUser;
                if (string.IsNullOrEmpty(u.PasswordHash))
                {
                    return false;
                } else
                {
                    return true;
                }
            }
        }

        public  async Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            
            var op = TableOperation.InsertOrMerge(user);
            var result = await Azure.userstoreTable.ExecuteAsync(op);            

        }

        public async Task<string> GetSecurityStampAsync(TUser user)
        {
            var op = TableOperation.Retrieve<User>("user", user.Id);
            var results = await Azure.userstoreTable.ExecuteAsync(op);
            if (results.Result == null)
            {
                return "";
            }
            else
            {
                return (results.Result as TUser).SecurityStamp;
            }
        }

        public async Task SetSecurityStampAsync(TUser user, string stamp)
        {
            user.SecurityStamp = stamp;

            var op = TableOperation.InsertOrMerge(user);
            var result = await Azure.userstoreTable.ExecuteAsync(op);  
        }

        public void Dispose()
        {
           
        }
    }
}