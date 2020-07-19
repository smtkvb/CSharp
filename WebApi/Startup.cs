using Microsoft.Examples;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Web.Http.Routing;
using Owin;
using Swashbuckle.Application;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using WebApi.Models;

[assembly: OwinStartup(typeof(WebApi.Startup))]
namespace WebApi
{
    public class Startup
    {
        
        public void Configuration(IAppBuilder builder)
        {
            
            var configuration = new HttpConfiguration();
            
            
            ConfigureOAuth(builder);
            builder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning(options => { 
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                int version;
                if (ConfigurationManager.AppSettings["DefaultVersion"] == null || int.TryParse(ConfigurationManager.AppSettings["DefaultVersion"], out version))
                { version = 1; }

                options.DefaultApiVersion = new Microsoft.Web.Http.ApiVersion(version, 0);
            });
            // we only need to change the default constraint resolver for services that want urls with versioning like: ~/v{version}/{controller}
            var constraintResolver = new DefaultInlineConstraintResolver() { ConstraintMap = { ["apiVersion"] = typeof(ApiVersionRouteConstraint) } };
            configuration.MapHttpAttributeRoutes(constraintResolver);

            // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. VersionedApiExplorer vs IApiExplorer)
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            var apiExplorer = configuration.AddVersionedApiExplorer(
                options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            configuration.EnableSwagger(
                "{apiVersion}",
                swagger =>
                {
                    swagger.RootUrl(req =>
                        req.RequestUri.GetLeftPart(UriPartial.Authority) +
                        req.GetRequestContext().VirtualPathRoot.TrimEnd('/'));
                    // build a swagger document and endpoint for each discovered API version
                    swagger.MultipleApiVersions(
                        (apiDescription, version) => apiDescription.GetGroupName()== version,
                        info =>
                        {
                            foreach (var group in apiExplorer.ApiDescriptions)
                            {
                                var description = "";

                                if (group.IsDeprecated)
                                {
                                    description += " This API version has been deprecated.";
                                }

                                info.Version(group.Name, $"API. Version {group.ApiVersion}")
                                    .Description(description);
                            }
                        });
                   
                    // add a custom operation filter which sets default values
                    swagger.OperationFilter<SwaggerDefaultValues>();
                    swagger.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();
                    // integrate xml comments
                    swagger.IncludeXmlComments(XmlCommentsFilePath);
                })
                .EnableSwaggerUi(swagger => {                    
                    swagger.EnableDiscoveryUrlSelector();
                    swagger.InjectStylesheet(typeof(Startup).Assembly, "WebApi.Content.SwaggerHeader.css");
                    swagger.DisableValidator();
                });

            var httpServer = new HttpServer(configuration);
            builder.UseWebApi(httpServer);

    
        }
        /*
        public void Configuration(IAppBuilder app)
        {
            /// ddd
            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
        }*/

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
                Provider = new TokenAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        static string XmlCommentsFilePath
        {
            get
            {
                return string.Format(@"{0}\bin\WebApi.xml", AppDomain.CurrentDomain.BaseDirectory);
            }
        }

    }
    public class TokenAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            /*commented for now
             Guid id = Guid.Parse(context.UserName);
                 var pro = db.Accounts.Where(o => o.id == id).FirstOrDefault();
                 if (pro == null)
                 {
                     context.SetError("invalid_grant", "incorrect.");
                     return;
                 }
     /*/
            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("UserName", context.UserName));
            identity.AddClaim(new Claim("Password", context.Password));
            context.Validated(identity);
        }
    }

}