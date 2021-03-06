

using System;
using System.Linq;
using Glass.Mapper.Pipelines.DataMapperResolver;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.DataMappers;
using NUnit.Framework;
using Sitecore.Data;
using Sitecore.FakeDb;
using Sitecore.SecurityModel;

namespace Glass.Mapper.Sc.FakeDb.DataMappers
{
    [TestFixture]
    public class AbstractSitecoreFieldMapperFixture 
    {
        #region Constructors

        [Test]
        public void Constructor_TypesPassed_TypesHandledSet()
        {
            //Assign
            var type1 = typeof (int);
            var type2 = typeof (string);

            //Act
            var mapper = new StubMapper(type1, type2);

            //Assert
            Assert.IsTrue(mapper.TypesHandled.Any(x => x == type1));
            Assert.IsTrue(mapper.TypesHandled.Any(x => x == type2));

        }

        #endregion

        #region Method - CanHandle

        [Test]
        public void CanHandle_TypeIsHandledWithConfig_ReturnsTrue()
        {
            //Assign
            var config = new SitecoreFieldConfiguration();
            var type1 = typeof (string);
            var mapper = new StubMapper(type1);

            config.PropertyInfo = typeof (Stub).GetProperty("Property");

            //Act
            var result = mapper.CanHandle(config, null);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanHandle_TwoTypesAreHandledWithConfig_ReturnsTrue()
        {
            //Assign
            var config = new SitecoreFieldConfiguration();
            var type2 = typeof(string);
            var type1 = typeof(int);
            var mapper = new StubMapper(type1, type2);

            config.PropertyInfo = typeof(Stub).GetProperty("Property");

            //Act
            var result = mapper.CanHandle(config, null);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CanHandle_IncorrectConfigType_ReturnsFalse()
        {
            //Assign
            var config = new SitecoreIdConfiguration();
            var type2 = typeof(string);
            var type1 = typeof(int);
            var mapper = new StubMapper(type1, type2);

            config.PropertyInfo = typeof(Stub).GetProperty("Property");

            //Act
            var result = mapper.CanHandle(config, null);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void CanHandle_IncorrectPropertType_ReturnsFalse()
        {
            //Assign
            var config = new SitecoreIdConfiguration();
            var type1 = typeof(int);
            var mapper = new StubMapper(type1);

            config.PropertyInfo = typeof(Stub).GetProperty("Property");

            //Act
            var result = mapper.CanHandle(config, null);

            //Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Method - MapToProperty

        [Test]
        public void MapToProperty_GetsValueByFieldName_ReturnsFieldValue()
        {
            //Assign
               var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                 new DbTemplate(templateId)
                 {
                     {"Field","" }
                 },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),
                
            })
            {
                var fieldValue = "test value";
                var fieldName = "Field";
                var item =
                    database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldName = fieldName;

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                var context = new SitecoreDataMappingContext(null, item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldName] = fieldValue;
                    item.Editing.EndEdit();
                }

                //Act
                var result = mapper.MapToProperty(context);

                //Assert
                Assert.AreEqual(fieldValue, result);

            }
        }

        [Test]
        public void MapToProperty_GetsValueByFieldId_ReturnsFieldValue()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {fieldId, ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value";
                var item =
                    database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldId = fieldId;

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                var context = new SitecoreDataMappingContext(null, item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldId] = fieldValue;
                    item.Editing.EndEdit();
                }

                //Act
                var result = mapper.MapToProperty(context);

                //Assert
                Assert.AreEqual(fieldValue, result);

            }
        }

        #endregion

        #region Method - MapToCms

        [Test]
        public void MapToCms_SetsValueByFieldId_FieldValueUpdated()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {fieldId, ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value set";
                var item = database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldId = fieldId;
                config.PropertyInfo = typeof(Stub).GetProperty("Property");

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                var context = new SitecoreDataMappingContext(new Stub(), item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldId] = string.Empty;
                    item.Editing.EndEdit();
                }

                //Act
                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    mapper.MapToCms(context);
                    item.Editing.EndEdit();

                }
                //Assert
                var itemAfter =
                    database.GetItem("/sitecore/content/Target");
                Assert.AreEqual(mapper.Value, itemAfter[fieldId]);
            }
        }

        [Test]
        public void MapToCms_SetsValueByFieldName_FieldValueUpdated()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {"Field", ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value set";
                var fieldName = "Field";
                var item = database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldName = fieldName;
                config.PropertyInfo = typeof(Stub).GetProperty("Property");

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                var context = new SitecoreDataMappingContext(new Stub(), item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldName] = string.Empty;
                    item.Editing.EndEdit();
                }

                //Act
                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    mapper.MapToCms(context);
                    item.Editing.EndEdit();

                }
                //Assert
                var itemAfter =
                    database.GetItem("/sitecore/content/Target");
                Assert.AreEqual(mapper.Value, itemAfter[fieldName]);
            }
        }

        

        #endregion

        #region Method - MapPropertyToCms

        [Test]
        public void MapPropertyToCms_FieldReadOnly_FieldNotUpdated()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {"Field", ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value set";
                var fieldName = "Field";
                var item = database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldName = fieldName;
                config.PropertyInfo = typeof(Stub).GetProperty("Property");
                config.ReadOnly = true;

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                Assert.IsTrue(mapper.ReadOnly);

                var context = new SitecoreDataMappingContext(new Stub(), item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldName] = string.Empty;
                    item.Editing.EndEdit();
                }

                //Act
                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    mapper.MapPropertyToCms(context);
                    item.Editing.EndEdit();

                }
                //Assert
                var itemAfter =
                    database.GetItem("/sitecore/content/Target");
                Assert.AreNotEqual(mapper.Value, itemAfter[fieldName]);
                Assert.AreEqual(item[fieldName], itemAfter[fieldName]);

            }
        }

        [Test]
        public void MapPropertyToCms_PageEditorOnly_FieldNotUpdated()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {"Field", ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value set";
                var fieldName = "Field";
                var item = database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldName = fieldName;
                config.PropertyInfo = typeof(Stub).GetProperty("Property");
                config.ReadOnly = false;
                config.Setting = SitecoreFieldSettings.PageEditorOnly;

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = fieldValue;

                Assert.IsFalse(mapper.ReadOnly);

                var context = new SitecoreDataMappingContext(new Stub(), item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldName] = string.Empty;
                    item.Editing.EndEdit();
                }

                //Act
                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    mapper.MapPropertyToCms(context);
                    item.Editing.EndEdit();

                }
                //Assert
                var itemAfter =
                    database.GetItem("/sitecore/content/Target");
                Assert.AreNotEqual(mapper.Value, itemAfter[fieldName]);
                Assert.AreEqual(item[fieldName], itemAfter[fieldName]);
            }
        }

        #endregion

        #region Method - MapCmsToProperty

        [Test]
        public void MapCmsToProperty_PageEditorOnly_FieldNotUpdated()
        {
            //Assign
            var templateId = ID.NewID;
            var fieldId = ID.NewID;
            var targetId = ID.NewID;
            using (Db database = new Db
            {
                new DbTemplate(templateId)
                {
                    {"Field", ""}
                },
                new Sitecore.FakeDb.DbItem("Target", targetId, templateId),

            })
            {
                var fieldValue = "test value set";
                var preValue = "some other value";
                var fieldName = "Field";
                var item = database.GetItem("/sitecore/content/Target");
                var options = new GetItemOptionsParams();

                var config = new SitecoreFieldConfiguration();
                config.FieldName = fieldName;
                config.PropertyInfo = typeof(Stub).GetProperty("Property");
                config.ReadOnly = false;
                config.Setting = SitecoreFieldSettings.PageEditorOnly;

                var mapper = new StubMapper(null);
                mapper.Setup(new DataMapperResolverArgs(null, config));
                mapper.Value = preValue;

                Assert.IsFalse(mapper.ReadOnly);

                var context = new SitecoreDataMappingContext(new Stub(), item, null, options);

                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    item[fieldName] = fieldValue;
                    item.Editing.EndEdit();
                }

                //Act
                using (new SecurityDisabler())
                {
                    item.Editing.BeginEdit();
                    mapper.MapCmsToProperty(context);
                    item.Editing.EndEdit();

                }
                //Assert
                var itemAfter =
                    database.GetItem("/sitecore/content/Target");
                Assert.AreEqual(mapper.Value, preValue);
                Assert.AreEqual(fieldValue, itemAfter[fieldName]);
            }
        }

        #endregion

        #region Stubs

        public class StubMapper : AbstractSitecoreFieldMapper
        {
            public string Value { get; set; }
            public StubMapper(params Type[] typeHandlers)
                : base(typeHandlers)
            {
            }

            public override object GetFieldValue(string fieldValue, Configuration.SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
            {
                return Value;
            }

            public override void SetField(Sitecore.Data.Fields.Field field, object value, Configuration.SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
            {
                field.Value = Value;
            }

            public override string SetFieldValue(object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class Stub
        {
            public string Property { get; set; }
        }

        #endregion

    }
}




