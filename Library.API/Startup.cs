﻿using AutoMapper;
using Library.API.Contexts;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Library.API
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
            services.AddMvc(setupAction =>
            {


                setupAction.ReturnHttpNotAcceptable = true;

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:LibraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));
            
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutingContext =
                        actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    if (actionContext.ModelState.ErrorCount > 0
                        && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            services.AddAutoMapper();


            //Swashbuckle is added here
            //A couple parameters are needed for the service adding swagger gen. These are added below
            //The "SwaggerDoc" helps with generating the documentation
            services.AddSwaggerGen(setupAction =>
            {
                //The SwaggerDoc needs a name and a model for the api documentation passed in as a parameter
                setupAction.SwaggerDoc("LibraryOpenAPISpecification", new Microsoft.OpenApi.Models.OpenApiInfo() 
                {
                    //The model needs specifications that can help identify the documentation
                   Title = "Library API",
                   Version = "1"
                });

                //We use reflection to get declare a variable name for the filepath of the xml file by using the variable below.
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                //We use "System.IO" to create the variable for the fullpath so it can be matched to the XML file
                //"Path.Combine" is used to concatenate the base directory of the project (whereever it is) with the variable that houses the XML comments file (whose variable is declared above) 
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                //The line of code below is used to ensure that the project can locate the xml file in order to improve the documentation 
                setupAction.IncludeXmlComments(xmlCommentsFullPath);
            });
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
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            //Adding swagger to the request pipeline is quite important too.
            //It is important for it to come after the "UseHttpsRedirection" pipeline request as it ensures that every unecrypted call will be redirected to the encrypted version

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                //we do this to show the swagger UI where to find the OpenAPI documentation and assign a name to the endpoint
                setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecification/swagger.json", "Library API");

                //To ensure that swagger loads in the root URL on launching the app, the code below is written
                setupAction.RoutePrefix = "";
            }); 

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
