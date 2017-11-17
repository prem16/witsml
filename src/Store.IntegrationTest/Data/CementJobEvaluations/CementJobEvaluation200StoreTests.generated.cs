//----------------------------------------------------------------------- 
// PDS WITSMLstudio Store, 2017.2
//
// Copyright 2017 PDS Americas LLC
// 
// Licensed under the PDS Open Source WITSML Product License Agreement (the
// "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.pds.group/WITSMLstudio/OpenSource/ProductLicenseAgreement
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

// ----------------------------------------------------------------------
// <auto-generated>
//     Changes to this file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
// ----------------------------------------------------------------------

using Energistics.DataAccess;
using Energistics.DataAccess.WITSML200;
using Energistics.DataAccess.WITSML200.ComponentSchemas;
using Energistics.DataAccess.WITSML200.ReferenceData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.WITSMLstudio.Store.Data.CementJobEvaluations
{
    [TestClass]
    public partial class CementJobEvaluation200StoreTests : CementJobEvaluation200TestBase
    {
        partial void BeforeEachTest();

        partial void AfterEachTest();

        protected override void OnTestSetUp()
        {
            BeforeEachTest();
        }

        protected override void OnTestCleanUp()
        {
            AfterEachTest();
        }

        [TestMethod]
        public void CementJobEvaluation200DataAdapter_GetFromStore_Can_Get_CementJobEvaluation()
        {
            AddParents();
            DevKit.AddAndAssert(CementJobEvaluation);
            DevKit.GetAndAssert(CementJobEvaluation);
       }

        [TestMethod]
        public void CementJobEvaluation200DataAdapter_AddToStore_Can_Add_CementJobEvaluation()
        {
            AddParents();
            DevKit.AddAndAssert(CementJobEvaluation);
        }

        [TestMethod]
        public void CementJobEvaluation200DataAdapter_UpdateInStore_Can_Update_CementJobEvaluation()
        {
            AddParents();
            DevKit.AddAndAssert(CementJobEvaluation);
            DevKit.UpdateAndAssert(CementJobEvaluation);
            DevKit.GetAndAssert(CementJobEvaluation);
        }

        [TestMethod]
        public void CementJobEvaluation200DataAdapter_DeleteFromStore_Can_Delete_CementJobEvaluation()
        {
            AddParents();
            DevKit.AddAndAssert(CementJobEvaluation);
            DevKit.DeleteAndAssert(CementJobEvaluation);
            DevKit.GetAndAssert(CementJobEvaluation, isNotNull: false);
        }
    }
}