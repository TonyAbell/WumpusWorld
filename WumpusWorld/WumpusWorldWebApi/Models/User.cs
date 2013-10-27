using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WumpusWorldWebApi.Models
{
    public class User : TableEntity, IUser
    {
        public User()
            : this(String.Empty)
        {
        }

        public User(string userName)
        {


            UserName = userName;
            Id = Guid.NewGuid().ToString();
            this.PartitionKey = "user";
            this.RowKey = Id;
            this.Roles = new List<UserRole>();
            this.Claims = new List<UserClaim>();
            
        }


        public string Id { get; set; }

        public string UserName { get; set; }

        public string ApiToken { get; set; }


        public virtual ICollection<UserClaim> Claims { get; set; }
        public virtual ICollection<UserLogin> Logins { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual ICollection<UserRole> Roles { get; set; }
        public virtual string SecurityStamp { get; set; }
       

    }
    public class UserClaim
    {     
        public virtual string ClaimType { get; set; }
        public virtual string ClaimValue { get; set; }
        public virtual int Id { get; set; }
       
    }
    public class UserLogin: TableEntity
    {
        public UserLogin()
        {

        }
        public UserLogin(string userId, string loginProvider, string providerKey)
        {
            this.PartitionKey = loginProvider;
            var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(providerKey));
            this.RowKey = rowKey;

            LoginProvider = loginProvider;
            ProviderKey = providerKey;
            UserId = userId;
        }
        public virtual string LoginProvider { get; set; }
        public virtual string ProviderKey { get; set; }    
        public virtual string UserId { get; set; }
    }
    public class UserRole
    {        
        public virtual Role Role { get; set; }
        public virtual string RoleId { get; set; }      
        public virtual string UserId { get; set; }
    }

    public class Role : IRole
    {
        public Role() { }
        public Role(string roleName)
        {
            this.Name = roleName;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }

    //public class UserLogin : Microsoft.WindowsAzure.Storage.Table.TableEntity, IUserLogin
    //{

    //    public string LoginProvider { get; set; }

    //    public string ProviderKey { get; set; }

    //    public string UserId { get; set; }

    //    public UserLogin() { }

    //    public UserLogin(string userId, string loginProvider, string providerKey)
    //    {
    //        this.PartitionKey = loginProvider;
    //        var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(providerKey));
    //        this.RowKey = rowKey;

    //        LoginProvider = loginProvider;
    //        ProviderKey = providerKey;
    //        UserId = userId;
    //    }
    //}

    //public class User : Microsoft.WindowsAzure.Storage.Table.TableEntity, IUser
    //{
    //    public User()
    //        : this(String.Empty)
    //    {
    //    }

    //    public User(string userName)
    //    {


    //        UserName = userName;
    //        Id = Guid.NewGuid().ToString();
    //        this.PartitionKey = "user";
    //        this.RowKey = Id;
    //    }


    //    public string Id { get; set; }

    //    public string UserName { get; set; }

    //    public string ApiToken { get; set; }
    //}
}