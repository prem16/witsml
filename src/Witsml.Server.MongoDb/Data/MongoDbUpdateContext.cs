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
using System.Reflection;
using MongoDB.Driver;
using PDS.Witsml.Data;

namespace PDS.Witsml.Server.Data
{
    public class MongoDbUpdateContext<T> : DataObjectNavigationContext
    {
        public MongoDbUpdateContext()
        {
            DataObjectType = typeof(T);
            PropertyInfoList = new List<PropertyInfo>();
            PropertyValueList = new List<object>();
            Update = null;           
        }

        public override Type DataObjectType { get; }

        public List<PropertyInfo> PropertyInfoList { get; }

        public List<object> PropertyValueList { get; }

        public UpdateDefinition<T> Update { get; set; }
    }
}
