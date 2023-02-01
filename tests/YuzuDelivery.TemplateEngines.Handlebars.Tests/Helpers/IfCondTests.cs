using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using YuzuDelivery.Core;

namespace YuzuDelivery.TemplateEngines.Handlebars.Helpers
{

    public class IfCondTests
    {

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            YuzuConstants.Reset();
            YuzuConstants.Initialize(new YuzuConstantsConfig());
        }

        [SetUp]
        public void Setup()
        {
            IfCond.Register();
            DynPartial.Register();
        }

        [Test]
        public void when_equals_true_then_show()
        {
            var source = "{{#ifCond foo '===' 'bar'}}true{{/ifCond}}";
            var template = HandlebarsDotNet.Handlebars.Compile(source);

            var data = new { foo = "bar" };

            var output = template(data);
            Assert.That(output, Is.EqualTo("true"));
        }

        [Test]
        public void when_not_equal_then_dont_show()
        {
            var source = "{{#ifCond foo '===' 'bar'}}true{{/ifCond}}";
            var template = HandlebarsDotNet.Handlebars.Compile(source);

            var data = new { foo = "test" };

            var output = template(data);
            Assert.That(output, Is.EqualTo(string.Empty));
        }

        [Test]
        public void when_else_exists_then_show_inverse()
        {
            var source = "{{#ifCond foo '===' 'bar'}}true{{else}}false{{/ifCond}}";
            var template = HandlebarsDotNet.Handlebars.Compile(source);

            var data = new { foo = "test" };

            var output = template(data);
            Assert.That(output, Is.EqualTo("false"));
        }

        [Test]
        public void with_a_dynPartial_in_an_ifCond_in_a_loop_then_the_parent_this_context_in_the_ifCond_passed_to_the_partial_should_be_the_root()
        {
            var source = @"{{#each loop}}
                                {{#ifCond another '===' 'value'}}
                                    {{{#dynPartial 'partial' ../../this}}}
                                {{/ifCond}}
                            {{/each}}";
            var partialSource = "test {{foo}}";
            var template = HandlebarsDotNet.Handlebars.Compile(source);

            using (var reader = new StringReader(partialSource))
            {
                var partialTemplate = HandlebarsDotNet.Handlebars.Compile(reader);
                HandlebarsDotNet.Handlebars.RegisterTemplate("partial", partialTemplate);
            }

            var data = new { foo = "bar", loop = new object[] { new { another = "value" } } };

            var output = template(data);
            Assert.That(output, Is.EqualTo("test bar"));
        }

        [Test]
        public void when_dyn_partial_is_in_ifCond_then_take_this_context()
        {
            var source = @"{{#ifCond sub.another '===' 'value'}}
                                {{{#dynPartial 'partial' this}}}
                            {{/ifCond}}";
            var partialSource = "test {{foo}}";
            var template = HandlebarsDotNet.Handlebars.Compile(source);

            using (var reader = new StringReader(partialSource))
            {
                var partialTemplate = HandlebarsDotNet.Handlebars.Compile(reader);
                HandlebarsDotNet.Handlebars.RegisterTemplate("partial", partialTemplate);
            }

            var data = new { foo = "bar", sub = new { another = "value" } };

            var output = template(data);
            Assert.That(output, Is.EqualTo("test bar"));
        }
    }
}
