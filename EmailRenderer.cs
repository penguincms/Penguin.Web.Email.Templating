using Microsoft.AspNetCore.Hosting;
using Penguin.DependencyInjection.Abstractions.Attributes;
using Penguin.DependencyInjection.Abstractions.Enums;
using Penguin.Email.Templating.Abstractions.Interfaces;
using Penguin.Templating.Abstractions;
using Penguin.Web.Mvc.Abstractions;
using Penguin.Web.Templating;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading.Tasks;

namespace Penguin.Web.Email.Templating
{
    /// <summary>
    /// An email template binder/renderer that uses the MVC view engine to allow for C# based email templating
    /// </summary>
    [Register(ServiceLifetime.Scoped, typeof(IEmailTemplateRenderer))]
    public class EmailTemplateRenderer : ObjectRenderer, IEmailTemplateRenderer
    {
        private IViewRenderService ViewRenderService { get; set; }

        /// <summary>
        /// Constructs a new instance of this templating engine
        /// </summary>
        /// <param name="viewRenderService">An implementation of a View Renderer for retrieving HTML</param>
        /// <param name="hostingEnvironment">An instance of a hosting environment for template generation</param>
        public EmailTemplateRenderer(IViewRenderService viewRenderService, IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            this.ViewRenderService = viewRenderService;
        }

        /// <summary>
        /// Takes information required by the email template and attempts to return bound HTML representing the provided field
        /// </summary>
        /// <param name="SourceValues">A collection of object names and values to be passed into the template</param>
        /// <param name="Template">The template to use for generation</param>
        /// <param name="Field">The field/property on the email template that this call is binding (in case its not the body)</param>
        /// <returns>The Html contents of the post-bound template field</returns>
        public string RenderEmail(IEnumerable<TemplateParameter> SourceValues, IEmailTemplate Template, PropertyInfo Field)
        {
            Contract.Requires(Field != null);

            string Body = Field.GetValue(Template)?.ToString();

            if (string.IsNullOrWhiteSpace(Body))
            { return Body; }

            GeneratedTemplateInfo info = base.GenerateTemplatePath(Template, SourceValues, Body, Field.Name);

            Task<string> renderTask = this.ViewRenderService.RenderToStringAsync(info.RelativePath, info.AbsolutePath.Replace(info.RelativePath, ""), info.Model, true);

            renderTask.Wait();

            return renderTask.Result.Trim();
        }
    }
}