﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using log4net;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Reflection;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Energistics.DataAccess.WITSML141.ComponentSchemas;

namespace PDS.Witsml.Server.Data
{
    /// <summary>
    /// MongoDb data adapter that encapsulates CRUD functionality for WITSML objects.
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <seealso cref="Data.WitsmlDataAdapter{T}" />
    public abstract class MongoDbDataAdapter<T> : WitsmlDataAdapter<T>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(MongoDbDataAdapter<T>));

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbDataAdapter{T}" /> class.
        /// </summary>
        /// <param name="databaseProvider">The database provider.</param>
        /// <param name="dbCollectionName">The database collection name.</param>
        /// <param name="idPropertyName">The name of the identifier property.</param>
        public MongoDbDataAdapter(IDatabaseProvider databaseProvider, string dbCollectionName, string idPropertyName = ObjectTypes.Uid)
        {
            DatabaseProvider = databaseProvider;
            DbCollectionName = dbCollectionName;
            IdPropertyName = idPropertyName;
        }

        /// <summary>
        /// Gets the database provider used for accessing MongoDb.
        /// </summary>
        /// <value>The database provider.</value>
        protected IDatabaseProvider DatabaseProvider { get; private set; }

        /// <summary>
        /// Gets the database collection name for the data object.
        /// </summary>
        /// <value>The database collection name.</value>
        protected string DbCollectionName { get; private set; }

        /// <summary>
        /// Gets the name of the identifier property.
        /// </summary>
        /// <value>The name of the identifier property.</value>
        protected string IdPropertyName { get; private set; }

        /// <summary>
        /// Gets a data object by the specified UUID.
        /// </summary>
        /// <param name="uuid">The UUID.</param>
        /// <returns>The data object instance.</returns>
        public override T Get(string uuid)
        {
            return GetEntity(uuid);
        }

        /// <summary>
        /// Determines whether the entity exists in the data store.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns>true if the entity exists; otherwise, false</returns>
        public override bool Exists(string uid)
        {
            return Exists<T>(uid, DbCollectionName);
        }

        /// <summary>
        /// Determines whether the entity exists in the data store.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <param name="dbCollectionName">The name of the database collection.</param>
        /// <typeparam name="TObject">The data object type.</typeparam>
        /// <returns>true if the entity exists; otherwise, false</returns>
        protected bool Exists<TObject>(string uid, string dbCollectionName)
        {
            try
            {
                return GetEntityByUidQuery<TObject>(uid, dbCollectionName).Any();
            }
            catch (MongoException ex)
            {
                _log.Error("Error querying " + dbCollectionName, ex);
                throw new WitsmlException(ErrorCodes.ErrorReadingFromDataStore, ex);
            }
        }

        protected IMongoCollection<T> GetCollection()
        {
            return GetCollection<T>(DbCollectionName);
        }

        protected IMongoCollection<TObject> GetCollection<TObject>(string dbCollectionName)
        {
            var database = DatabaseProvider.GetDatabase();
            return database.GetCollection<TObject>(dbCollectionName);
        }

        protected IMongoQueryable<T> GetQuery()
        {
            return GetQuery<T>(DbCollectionName);
        }

        protected IMongoQueryable<TObject> GetQuery<TObject>(string dbCollectionName)
        {
            return GetCollection<TObject>(dbCollectionName).AsQueryable();
        }

        /// <summary>
        /// Gets an object from the data store by uid
        /// </summary>
        /// <param name="uid">The uid of the object.</param>
        /// <returns>The object represented by the UID.</returns>
        protected T GetEntity(string uid)
        {
            return GetEntity<T>(uid, DbCollectionName);
        }

        /// <summary>
        /// Gets an object from the data store by uid
        /// </summary>
        /// <param name="uid">The uid of the object.</param>
        /// <param name="dbCollectionName">The naame of the database collection.</param>
        /// <typeparam name="TObject">The data object type.</typeparam>
        /// <returns>The object represented by the UID.</returns>
        protected TObject GetEntity<TObject>(string uid, string dbCollectionName)
        {
            try
            {
                _log.DebugFormat("Query WITSML object: {0}; uid: {1}", dbCollectionName, uid);
                return GetEntityByUidQuery<TObject>(uid, dbCollectionName).FirstOrDefault();
            }
            catch (MongoException ex)
            {
                _log.Error("Error querying " + dbCollectionName, ex);
                throw new WitsmlException(ErrorCodes.ErrorReadingFromDataStore, ex);
            }
        }

        protected IQueryable<T> GetEntityByUidQuery(string uid)
        {
            return GetEntityByUidQuery<T>(uid, DbCollectionName);
        }

        protected IQueryable<TObject> GetEntityByUidQuery<TObject>(string uid, string dbCollectionName)
        {
            var query = GetQuery<TObject>(dbCollectionName)
                .Where(IdPropertyName + " = @0", uid);

            return query;
        }

        protected void FillObjectTemplateValues(Type objectType, object dataObject)
        {
            PropertyInfo[] propertyInfo = objectType.GetProperties();
            foreach (PropertyInfo property in propertyInfo)
            {
                Type propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    Type type = propertyType.GetGenericArguments()[0];
                    FillObjectTypeValues(dataObject, objectType, property, type);
                    string specifiedPropertyName = property.Name + "Specified";
                    PropertyInfo specifiedProperty = objectType.GetProperty(specifiedPropertyName);
                    if (specifiedProperty!=null)
                    {
                        specifiedProperty.SetValue(dataObject, true);
                    }
                }
                else if (property.Name.ToLower().Equals("TimeZone".ToLower()))
                    property.SetValue(dataObject, "Z");
                else if (property.Name.ToLower().Equals("Date".ToLower()))
                    property.SetValue(dataObject, "1900-01-01");
                else if (property.Name.ToLower().Equals("CalendarYear".ToLower()))
                    property.SetValue(dataObject, "1000");
                else if (property.Name.ToLower().Equals("iadcBearingWearCode".ToLower()))
                    property.SetValue(dataObject, "E");
                else if (property.Name.ToLower().Equals("geodeticZoneString".ToLower()))
                    property.SetValue(dataObject, "60N");
                else if (property.Name.ToLower().Equals("sectionNumber".ToLower()))
                    property.SetValue(dataObject, "36");
                else if (property.Name.ToLower().Equals("publicLandSurveySystemQuarterTownship".ToLower()))
                    property.SetValue(dataObject, "NE");
                else if (property.Name.ToLower().Equals("publicLandSurveySystemQuarterSection".ToLower()))
                    property.SetValue(dataObject, "NE");
                else if (property.Name.ToLower().Equals("number".ToLower()))
                    property.SetValue(dataObject, 1);
                else
                    FillObjectTypeValues(dataObject, objectType, property, property.PropertyType);
            }
        }

        private void FillObjectTypeValues(object dataObject, Type objectType, PropertyInfo property, Type propertyType)
        {
            if (propertyType == typeof(string))
                property.SetValue(dataObject, "abc");
            else if (propertyType == typeof(bool))
            {
                int index = property.Name.LastIndexOf("Specified");
                if (index < 0)
                    property.SetValue(dataObject, false);
                else
                {
                    string specifiedNameSubstring = property.Name.Substring(index, property.Name.Length - index);
                    bool isSpecifiedProperty = specifiedNameSubstring.Equals("Specified");
                    if (!isSpecifiedProperty)
                        property.SetValue(dataObject, false);
                }
            }
            else if (propertyType == typeof(DateTime))
                property.SetValue(dataObject, Convert.ToDateTime("1900-01-01T00:00:00.000Z"));
            else if (propertyType == typeof(WellStatus))
                property.SetValue(dataObject, WellStatus.unknown);
            else if (propertyType == typeof(WellPurpose))
                property.SetValue(dataObject, WellPurpose.unknown);
            else if (propertyType == typeof(WellFluid))
                property.SetValue(dataObject, WellFluid.unknown);
            else if (propertyType == typeof(WellDirection))
                property.SetValue(dataObject, WellDirection.unknown);
            else if (propertyType == typeof(LengthUom))
                property.SetValue(dataObject, LengthUom.ft);
            else if (propertyType == typeof(LengthMeasure))
                property.SetValue(dataObject, new LengthMeasure(1.0, LengthUom.ft));
            else if (propertyType == typeof(DimensionlessMeasure))
                property.SetValue(dataObject, new DimensionlessMeasure(1.0, DimensionlessUom.Item));
            else if (propertyType == typeof(List<WellDatum>))
            {
                WellDatum datum = new WellDatum();
                FillObjectTemplateValues(typeof(WellDatum), datum);
                property.SetValue(dataObject, new List<WellDatum>() { datum });
            }
            else if (propertyType == typeof(List<Location>))
            {
                Location location = new Location();
                location.Easting = new LengthMeasure(1.0, LengthUom.ft);
                property.SetValue(dataObject, new List<Location>() { location });
            }
            else if (propertyType == typeof(List<ReferencePoint>))
            {
                ReferencePoint referencePoint = new ReferencePoint();
                FillObjectTemplateValues(typeof(ReferencePoint), referencePoint);
                property.SetValue(dataObject, new List<ReferencePoint>() { referencePoint });
            }
            else if (propertyType == typeof(List<WellCRS>))
            {
                WellCRS wellCRS = new WellCRS();
                FillObjectTemplateValues(typeof(WellCRS), wellCRS);
                property.SetValue(dataObject, new List<WellCRS>() { wellCRS });
            }
            else if (propertyType == typeof(CommonData))
            {
                CommonData commonData = new CommonData();
                FillObjectTemplateValues(typeof(CommonData), commonData);
                property.SetValue(dataObject, commonData);
            }
        }

        /// <summary>
        /// Queries the data store.
        /// </summary>
        /// <param name="parser">The query parser.</param>
        /// <param name="names">The property names.</param>
        /// <returns>A collection of data objects.</returns>
        protected List<T> QueryEntities(WitsmlQueryParser parser, List<string> names)
        {
            return QueryEntities<T>(parser, DbCollectionName, names);
        }

        /// <summary>
        /// Queries the data store.
        /// </summary>
        /// <param name="parser">The query parser.</param>
        /// <param name="dbCollectionName">Name of the database collection.</param>
        /// <param name="names">The property names.</param>
        /// <typeparam name="TObject">The data object type.</typeparam>
        /// <returns>A collection of data objects.</returns>
        protected List<TObject> QueryEntities<TObject>(WitsmlQueryParser parser, string dbCollectionName, List<string> names)
        {
            // Find a unique entity by Uid if one was provided
            var uid = parser.Attribute("uid");

            if (!string.IsNullOrEmpty(uid))
            {
                return GetEntityByUidQuery<TObject>(uid, dbCollectionName).ToList();
            }

            // Default to return all entities
            var query = GetQuery<TObject>(dbCollectionName);

            //... filter by unique name list if values for 
            //... each name can be parsed from the Witsml query.
            return FilterQuery(parser, query, names).ToList();
        }

        /// <summary>
        /// Inserts an object into the data store.
        /// </summary>
        /// <param name="entity">The object to be inserted.</param>
        protected void InsertEntity(T entity)
        {
            InsertEntity(entity, DbCollectionName);
        }

        /// <summary>
        /// Inserts an object into the data store.
        /// </summary>
        /// <param name="entity">The object to be inserted.</param>
        /// <param name="dbCollectionName">The name of the database collection.</param>
        /// <typeparam name="TObject">The data object type.</typeparam>
        protected void InsertEntity<TObject>(TObject entity, string dbCollectionName)
        {
            try
            {
                _log.DebugFormat("Insert WITSML object: {0}", dbCollectionName);

                var collection = GetCollection<TObject>(dbCollectionName);

                collection.InsertOne(entity);
            }
            catch (MongoException ex)
            {
                _log.Error("Error inserting " + dbCollectionName, ex);
                throw new WitsmlException(ErrorCodes.ErrorAddingToDataStore, ex);
            }
        }

        /// <summary>
        /// Initializes a new UID value if one was not supplied.
        /// </summary>
        /// <param name="uid">The supplied UID (default value null).</param>
        /// <returns>The supplied UID if not null; otherwise, a generated UID.</returns>
        protected string NewUid(string uid = null)
        {
            if (string.IsNullOrEmpty(uid))
            {
                uid = Guid.NewGuid().ToString();
            }

            return uid;
        }

        protected IQueryable<TObject> FilterQuery<TObject>(WitsmlQueryParser parser, IQueryable<TObject> query, List<string> names)
        {
            // For entity property name and its value
            var nameValues = new Dictionary<string, string>();

            // For each name pair ("<xml name>,<entity propety name>") 
            //... create a dictionary of property names and corresponding values.
            names.ForEach(n =>
            {
                // Split out the xml name and entity property names for ease of use.
                var nameAndProperty = n.Split(',');
                nameValues.Add(nameAndProperty[1], parser.PropertyValue(nameAndProperty[0]));
            });

            query = QueryByNames(query, nameValues);

            return query;
        }

        protected IQueryable<TObject> QueryByNames<TObject>(IQueryable<TObject> query, Dictionary<string, string> nameValues)
        {
            if (nameValues.Values.ToList().TrueForAll(nameValue => !string.IsNullOrEmpty(nameValue)))
            {
                nameValues.Keys.ToList().ForEach(nameKey =>
                {
                    query = query.Where(string.Format("{0} = \"{1}\"", nameKey, nameValues[nameKey]));
                });
            }

            return query;
        }
    }
}
