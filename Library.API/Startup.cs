using AutoMapper;
using Library.API.Authentication;
using Library.API.Contexts;
using Library.API.OperationsFilters;
using Library.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

//The line of code below shows how to declare default API conventions globally
[assembly: ApiConventionType(typeof(DefaultApiConventions))]
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
                //Using this "setupAction.Filter.Add" helps to globally declare response types for APIs
                //It's important to note that the attributes declared below instantly overrides any default convention declared anywhere in the project
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                setupAction.Filters.Add(
                        new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                setupAction.Filters.Add(
                    new ProducesDefaultResponseTypeAttribute());
                setupAction.Filters.Add(
                    new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized));
                
                //Adding this completes the process of creating the authentication
                setupAction.Filters.Add(
                    new AuthorizeFilter());

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

            //This will help with writing documentation for particular API
            services.AddVersionedApiExplorer(setupAction=>
            {
                //Just like when we addressed multiple documentation, this works in such a way that it finds the API version and any minor version
                setupAction.GroupNameFormat = "'v'VV";
            });

            //This is where we declare the authentication and tie it to the Basic Authentication Handler class
            services.AddAuthentication("Basic").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

            //The service below ensures that API can be versioned in this project.
            //Note that a default API must be specified at least for the API to be consumable.
            services.AddApiVersioning(setupAction=>
            {
                //The SetupAction below helps the project figure our the default version of the API so we don't have to pass it in.
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                //The declaration below is where we declare the default version of the API (in this case it's 1.0)
                //This Default API is a combined version in that it can detect the API regardless of the style of declaration
                setupAction.DefaultApiVersion = new ApiVersion(1, 0);
                //The middleware below reports the API back to us so we know
                setupAction.ReportApiVersions = true;
                //This helps the project read the header of the API in order to know the version of the API
                //setupAction.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //For media type APIs, we can use the middleware below.
                //setupAction.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
            });

            //We use this to get the individual API descriptions so we can identify them.
            //It must only come after the "AddApiVersioning" service.
            var apiVersionDescriptionProvider = services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

            //Swashbuckle is added here
            //A couple parameters are needed for the service adding swagger gen. These are added below
            //The "SwaggerDoc" helps with generating the documentation
            services.AddSwaggerGen(setupAction =>
            {
                //This helps us identify each api by description
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    //The SwaggerDoc needs a name and a model for the api documentation passed in as a parameter
                    //We are passing the groupname in here to identify the API
                    setupAction.SwaggerDoc($"LibraryOpenAPISpecification{description.GroupName}", new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        //The model needs specifications that can help identify the documentation
                        Title = "Library API",
                        //We just set the version to recieve a string with its version name
                        Version = description.ApiVersion.ToString(),
                        //It helps to attach descriptions to API especially when you're exposing them to the public.
                        Description = "Through this API you can access authors and their books.",
                        //It is also important to add contact information to your API in order to help users of the API send feedback.
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "olabayobalogun@gmail.com",
                            Name = " Olabayo Balogun",
                            Url = new Uri("https://www.linkedin.com/in/olabayobalogun/")
                            //The extensions property can be used to hint at features that aren't covered by Open API documentation, eg, address, logo, etc.
                            //Extensions = 
                        },
                        //The License feature can be used to show things like licenses
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensources.org/licenses/MIT")
                        }
                        //Terms of service can also be added if needed.
                        //TermsOfService =
                    });
                }

                ////The SwaggerDoc needs a name and a model for the api documentation passed in as a parameter
                //setupAction.SwaggerDoc("LibraryOpenAPISpecification", new Microsoft.OpenApi.Models.OpenApiInfo()
                //{
                //    //The model needs specifications that can help identify the documentation
                //    Title = "Library API",
                //    Version = "1",
                //    //It helps to attach descriptions to API especially when you're exposing them to the public.
                //    Description = "Through this API you can access authors and their books.",
                //    //It is also important to add contact information to your API in order to help users of the API send feedback.
                //    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                //    {
                //        Email = "olabayobalogun@gmail.com",
                //        Name = " Olabayo Balogun",
                //        Url = new Uri("https://www.linkedin.com/in/olabayobalogun/")
                //        //The extensions property can be used to hint at features that aren't covered by Open API documentation, eg, address, logo, etc.
                //        //Extensions = 
                //    },
                //    //The License feature can be used to show things like licenses
                //    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                //    {
                //        Name = "MIT License",
                //        Url = new Uri("https://opensources.org/licenses/MIT")
                //    }
                //    //Terms of service can also be added if needed.
                //    //TermsOfService =
                //});

                //This swagger document and the one after it are used to show how to work with multiple OpenAPI specifications.
                //setupAction.SwaggerDoc("LibraryOpenAPISpecificationAuthors", new Microsoft.OpenApi.Models.OpenApiInfo()
                //{
                //    //The model needs specifications that can help identify the documentation
                //    Title = "Library API (Authors)",
                //    Version = "1",
                //    //It helps to attach descriptions to API especially when you're exposing them to the public.
                //    Description = "Through this API you can access authors.",
                //    //It is also important to add contact information to your API in order to help users of the API send feedback.
                //    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                //    {
                //        Email = "olabayobalogun@gmail.com",
                //        Name = " Olabayo Balogun",
                //        Url = new Uri("https://www.linkedin.com/in/olabayobalogun/")
                //        //The extensions property can be used to hint at features that aren't covered by Open API documentation, eg, address, logo, etc.
                //        //Extensions = 
                //    },
                //    //The License feature can be used to show things like licenses
                //    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                //    {
                //        Name = "MIT License",
                //        Url = new Uri("https://opensources.org/licenses/MIT")
                //    }
                //    //Terms of service can also be added if needed.
                //    //TermsOfService =
                //});

                //This second swagger document is meant to show how we can work with multiple Open API specifications
                //setupAction.SwaggerDoc("LibraryOpenAPISpecificationBooks", new Microsoft.OpenApi.Models.OpenApiInfo()
                //{
                //    //The model needs specifications that can help identify the documentation
                //    Title = "Library API (Books)",
                //    Version = "1",
                //    //It helps to attach descriptions to API especially when you're exposing them to the public.
                //    Description = "Through this API you can access books.",
                //    //It is also important to add contact information to your API in order to help users of the API send feedback.
                //    Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                //    {
                //        Email = "olabayobalogun@gmail.com",
                //        Name = " Olabayo Balogun",
                //        Url = new Uri("https://www.linkedin.com/in/olabayobalogun/")
                //        //The extensions property can be used to hint at features that aren't covered by Open API documentation, eg, address, logo, etc.
                //        //Extensions = 
                //    },
                //    //The License feature can be used to show things like licenses
                //    License = new Microsoft.OpenApi.Models.OpenApiLicense()
                //    {
                //        Name = "MIT License",
                //        Url = new Uri("https://opensources.org/licenses/MIT")
                //    }
                //    //Terms of service can also be added if needed.
                //    //TermsOfService =
                //});

                setupAction.AddSecurityDefinition("basicAuth", new OpenApiSecurityScheme()
                {
                    //basicAuth requires you to pass in the type of the security definition, the "Scheme" is case sensitive
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    Description = "Input your username and password to access this API"
                });

                //This is where we declare the security authentication as one that requires authentication
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "basicAuth" }
                        }, new List<string>() }
                });

                //This helps us with a strategy for selecting actions
                //This middleware can also make it possible for documentation to appear in a version regardless of it has the action or now, as long as one of the APIs have it.
                setupAction.DocInclusionPredicate((documentName, apiDescription) =>
                {
                    var actionApiVersionModel = apiDescription.ActionDescriptor.GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }

                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                        $"LibraryOpenApiSpecificationv{v.ToString()}" == documentName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"LibraryOpenAPISpecification{v.ToString()}" == documentName);
                });

                //The service below is called in the situation where you need to differentiate between two APIs that have very similar names and attributes
                /*setupAction.ResolveConflictingActions(apiDescriptions =>
                {
                    return apiDescriptions.First();
                });*/

                //We use the service below register the operation filter and to ensure that the Operation Filter works
                setupAction.OperationFilter<GetBookOperationFilter>();
                setupAction.OperationFilter<CreateBookOperationFilter>();

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
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

            //app.UseSwaggerUI(setupAction =>
            //{
            //    //we do this to show the swagger UI where to find the OpenAPI documentation and assign a name to the endpoint
            //    setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecification/swagger.json", "Library API");

            //    //To ensure that swagger loads in the root URL on launching the app, the code below is written
            //    setupAction.RoutePrefix = "";
            //}); 

            app.UseSwaggerUI(setupAction =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    //we do this to show the swagger UI where to find the OpenAPI documentation and assign a name to the endpoint
                    //this helps in setting up the view for each API according to their version number
                    setupAction.SwaggerEndpoint($"/swagger/LibraryOpenAPISpecification{description.GroupName}/swagger.json", "Library API");

                    //setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationAuthors/swagger.json", "Library API (Authors)");

                    //When working with multiple OpenAPI specifications, you can add their locations here like this.
                    //Having multiple API specifications may clash with API versioning.
                    //setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationBooks/swagger.json", "Library API (Books)");
                    //To ensure that swagger loads in the root URL on launching the app, the code below is written
                    setupAction.RoutePrefix = "";

                    //The code below helps to set the depth to which a model is expanded
                    setupAction.DefaultModelExpandDepth(2);
                    setupAction.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                    //The code below controls the expansion for operations and tags
                    setupAction.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                }
                ////we do this to show the swagger UI where to find the OpenAPI documentation and assign a name to the endpoint
                //setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecification/swagger.json", "Library API");
                
                ////setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationAuthors/swagger.json", "Library API (Authors)");

                ////When working with multiple OpenAPI specifications, you can add their locations here like this.
                ////Having multiple API specifications may clash with API versioning.
                ////setupAction.SwaggerEndpoint("/swagger/LibraryOpenAPISpecificationBooks/swagger.json", "Library API (Books)");
                ////To ensure that swagger loads in the root URL on launching the app, the code below is written
                //setupAction.RoutePrefix = "";
            });

            app.UseStaticFiles();

            //Make sure it occurs before the "UseMvc" call
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
