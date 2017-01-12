﻿using System;
using System.Text;
using System.Web;
using SFA.DAS.EAS.Domain;

namespace SFA.DAS.EAS.Web
{
    public interface ICookieService
    {
        void Create(HttpContextBase context, string name, string content, int expireDays);
        void Update(HttpContextBase context, string name, string content);
        void Delete(HttpContextBase context, string name);
        object Get(HttpContextBase context, string name);
    }

    public class HttpCookieService : ICookieService
    {
        public void Create(HttpContextBase context, string name, string content, int expireDays)
        {

            var plainTextBytes = Encoding.UTF8.GetBytes(content);
            var encodedContent = Convert.ToBase64String(plainTextBytes);

            var userCookie = new HttpCookie(name, encodedContent)
            {
                Expires = DateTime.Now.AddDays(expireDays),
                Secure = true,
                HttpOnly = true,
            };

            context.Response.Cookies.Add(userCookie);
        }

        public void Update(HttpContextBase context, string name, string content)
        {
            var cookie = context.Request.Cookies[name];

            var plainTextBytes = Encoding.UTF8.GetBytes(content);
            var encodedContent = Convert.ToBase64String(plainTextBytes);

            if (cookie != null)
            {
                cookie.Value = encodedContent;
                
                context.Response.SetCookie(cookie);
            }
        }

        public void Delete(HttpContextBase context, string name)
        {
            if (context.Request.Cookies[name] != null)
            {
                var user = new HttpCookie(name)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Value = null
                };
                context.Response.SetCookie(user);
            }
        }

        public object Get(HttpContextBase context, string name)
        {
            if (context.Request.Cookies[name] == null)
                return null;

            var base64EncodedBytes = System.Convert.FromBase64String(context.Request.Cookies[name].Value);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class EmployerAccountData
    {
        public OrganisationType OrganisationType { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationReferenceNumber { get; set; }
        public string OrganisationRegisteredAddress { get; set; }
        public DateTime OrganisationDateOfInception { get; set; }

        public string PayeReference { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool EmpRefNotFound { get; set; }
        public bool HideBreadcrumb { get; set; }
        public string OrganisationStatus { get; set; }
        public string EmployerRefName { get; set; }
    }
}