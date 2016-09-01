//----------------------------------------------------------------------- 
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
using Energistics.DataAccess.WITSML131.ReferenceData;

namespace PDS.Witsml.Server.Data.Messages
{
    /// <summary>
    /// Message131TestBase
    /// </summary>
    public partial class Message131TestBase
    {
        partial void BeforeEachTest()
        {
            Message.DateTime = DateTime.Now;
            Message.TypeMessage = MessageType.informational;
        }
    }
}