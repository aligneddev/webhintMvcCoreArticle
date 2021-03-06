﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace webhintMvcCoreArticle
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            this.RemoveAndSetSecurityRelatedHeaders(app);

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    Console.WriteLine("Header " + ctx.Context.Response.Headers["Content-Type"]);
                    Console.WriteLine(ctx.Context.Request.Path);
                    Console.WriteLine();
                    Console.WriteLine();
                    if (ctx.Context.Request.Path.HasValue)
                    {
                        if(ctx.Context.Request.Path.Value.ToLower().Contains(".ico")){
                            // don't change for .ico files
                            return;
                        }
                        else  if(ctx.Context.Request.Path.Value.ToLower().Contains(".js")){
                            var newContentType = "text/javascript; charset=utf-8";                            
                            ctx.Context.Response.Headers.Remove("Content-Type");
                            ctx.Context.Response.Headers.Append("Content-Type", newContentType);
                            return;
                        }

                        if (ctx.Context.Response.Headers.TryGetValue("Content-Type", out var header))
                        {
                            var newContentType = ctx.Context.Response.Headers["Content-Type"] += "; charset=utf-8";
                            ctx.Context.Response.Headers.Remove("Content-Type");
                            ctx.Context.Response.Headers.Append("Content-Type", newContentType);
                        }
                        else
                        {
                            ctx.Context.Response.Headers.Append("Content-Type", "charset=utf-8");
                            Console.WriteLine("new header");
                        }
                        Console.WriteLine("after Header " + ctx.Context.Response.Headers["Content-Type"]);
                        Console.WriteLine("--------------------------");
                    }
                }
            });

            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }


        private void RemoveAndSetSecurityRelatedHeaders(IApplicationBuilder app)
        {
            // Registered before static files to always set header

            // 'strict-transport-security' header 'max-age' value should be more than 10886400
            app.UseHsts(options => options.MaxAge(days: 180).IncludeSubdomains());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());
            app.UseCsp(opts => opts
            .BlockAllMixedContent()
            .StyleSources(s => s.Self())
            .StyleSources(s => s.UnsafeInline())
            .FontSources(s => s.Self())
            .FormActions(s => s.Self())
            .FrameAncestors(s => s.Self())
            .ImageSources(s => s.Self())
            .ScriptSources(s => s.Self())
        );
        }
    }
}
