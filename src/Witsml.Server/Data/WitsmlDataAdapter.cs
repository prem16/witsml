﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Energistics.DataAccess;
using Energistics.Datatypes;
using log4net;

namespace PDS.Witsml.Server.Data
{
    /// <summary>
    /// Data adapter that encapsulates CRUD functionality for WITSML data objects.
    /// </summary>
    /// <typeparam name="T">Type of the object.</typeparam>
    /// <seealso cref="PDS.Witsml.Server.Data.IWitsmlDataAdapter{T}" />
    public abstract class WitsmlDataAdapter<T> : IWitsmlDataAdapter<T>
    {
        protected WitsmlDataAdapter()
        {
            Logger = LogManager.GetLogger(GetType());
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILog Logger { get; }

        /// <summary>
        /// Retrieves data objects from the data store using the specified parser.
        /// </summary>
        /// <param name="parser">The query template parser.</param>
        /// <returns>
        /// A collection of data objects retrieved from the data store.
        /// </returns>
        public virtual List<T> Query(WitsmlQueryParser parser)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a data object to the data store.
        /// </summary>
        /// <param name="parser">The input template parser.</param>
        /// <param name="dataObject">The data object to be added.</param>
        public virtual void Add(WitsmlQueryParser parser, T dataObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a data object in the data store.
        /// </summary>
        /// <param name="parser">The input template parser.</param>
        /// <param name="dataObject">The data object to be updated.</param>
        public virtual void Update(WitsmlQueryParser parser, T dataObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes or partially updates the specified object in the data store.
        /// </summary>
        /// <param name="parser">The input template parser.</param>
        public virtual void Delete(WitsmlQueryParser parser)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the entity exists in the data store.
        /// </summary>
        /// <param name="uri">The data object URI.</param>
        /// <returns>true if the entity exists; otherwise, false</returns>
        public virtual bool Exists(EtpUri uri)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of data objects related to the specified URI.
        /// </summary>
        /// <param name="parentUri">The parent URI.</param>
        /// <returns>A collection of data objects.</returns>
        public virtual List<T> GetAll(EtpUri? parentUri = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a data object by the specified URI.
        /// </summary>
        /// <param name="uri">The data object URI.</param>
        /// <returns>The data object instance.</returns>
        public virtual T Get(EtpUri uri)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a data object by the specified URI.
        /// </summary>
        /// <param name="uri">The data object URI.</param>
        public virtual void Delete(EtpUri uri)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates the input template using the specified parser.
        /// </summary>
        /// <param name="parser">The query parser.</param>
        public virtual void Validate(WitsmlQueryParser parser)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates the growing object data request.
        /// </summary>
        /// <param name="parser">The query parser.</param>
        /// <param name="dataObjects">The data object headers.</param>
        protected virtual void ValidateGrowingObjectDataRequest(WitsmlQueryParser parser, List<T> dataObjects)
        {
            var queryCount = parser.Elements().Count();
            if (dataObjects.Count > queryCount)
            {
                throw new WitsmlException(ErrorCodes.MissingSubsetOfGrowingDataObject);
            }
        }

        /// <summary>
        /// Gets a list of the property names to project during a query.
        /// </summary>
        /// <param name="parser">The WITSML parser.</param>
        /// <returns>A list of property names.</returns>
        protected virtual List<string> GetProjectionPropertyNames(WitsmlQueryParser parser)
        {
            return null;
        }

        /// <summary>
        /// Gets a list of the element names to ignore during a query.
        /// </summary>
        /// <param name="parser">The WITSML parser.</param>
        /// <returns>A list of element names.</returns>
        protected virtual List<string> GetIgnoredElementNamesForQuery(WitsmlQueryParser parser)
        {
            return null;
        }

        /// <summary>
        /// Gets a list of the element names to ignore during an update.
        /// </summary>
        /// <param name="parser">The WITSML parser.</param>
        /// <returns>A list of element names.</returns>
        protected virtual List<string> GetIgnoredElementNamesForUpdate(WitsmlQueryParser parser)
        {
            return null;
        }

        /// <summary>
        /// Creates the query template.
        /// </summary>
        /// <returns>A query template.</returns>
        protected virtual WitsmlQueryTemplate<T> CreateQueryTemplate()
        {
            return new WitsmlQueryTemplate<T>();
        }

        /// <summary>
        /// Gets the URI for the specified data object.
        /// </summary>
        /// <param name="instance">The data object.</param>
        /// <returns>The URI representing the data object.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected virtual EtpUri GetUri(T instance)
        {
            var wellboreObject = instance as IWellboreObject;
            if (wellboreObject != null) return wellboreObject.GetUri();

            var wellObject = instance as IWellObject;
            if (wellObject != null) return wellObject.GetUri();

            var dataObject = instance as IDataObject;
            if (dataObject != null) return dataObject.GetUri();

            var abstractObject = instance as Energistics.DataAccess.WITSML200.ComponentSchemas.AbstractObject;
            if (abstractObject != null) return abstractObject.GetUri();

            throw new InvalidOperationException();
        }
    }
}
