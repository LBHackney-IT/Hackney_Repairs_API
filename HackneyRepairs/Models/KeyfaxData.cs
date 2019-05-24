//Auto generated class
namespace HackneyRepairs.Models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class KeyfaxData
    {
        private KeyfaxDataFault faultField;

        private string gUIDField;

        private byte statusField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public KeyfaxDataFault Fault
        {
            get
            {
                return this.faultField;
            }
            set
            {
                this.faultField = value;
            }
        }

        /// <remarks/>
        public string GUID
        {
            get
            {
                return this.gUIDField;
            }
            set
            {
                this.gUIDField = value;
            }
        }

        /// <remarks/>
        public byte Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class KeyfaxDataFault
    {
        private string isDeletedField;

        private string isDirtyField;

        private string isNewField;

        private string fault_DialogField;

        private string fault_CategoryField;

        private byte scriptSetField;

        private string faultTextField;

        private string logDateField;

        private object communal_TypeField;

        private object reason_CodeField;

        private object reason_TextField;

        private byte rechargeField;

        private string recharge_CodeField;

        private object recharge_CostField;

        private object repair_NoField;

        private byte repair_StatusField;

        private string schedule_IDField;

        private object site_VisitField;

        private object additional_InfoField;

        private object special_InstructionsField;

        private byte tenancy_TypeField;

        private object clientIDField;

        private object tenantIDField;

        private object tenantTextField;

        private string userCodeField;

        private string scriptPathField;

        private object contractor_CodeField;

        private string expenditure_CodeField;

        private object nominal_CodeField;

        private object job_CodeField;

        private object locationField;

        private byte repairCountField;

        private byte adviceCountField;

        private byte customCountField;

        private byte actionCountField;

        private byte subActionCountField;

        private byte updateCountField;

        private KeyfaxDataFaultRepair repairField;

        private KeyfaxDataFaultAdvice adviceField;

        private string nameField;

        private string typeField;

        /// <remarks/>
        public string IsDeleted
        {
            get
            {
                return this.isDeletedField;
            }
            set
            {
                this.isDeletedField = value;
            }
        }

        /// <remarks/>
        public string IsDirty
        {
            get
            {
                return this.isDirtyField;
            }
            set
            {
                this.isDirtyField = value;
            }
        }

        /// <remarks/>
        public string IsNew
        {
            get
            {
                return this.isNewField;
            }
            set
            {
                this.isNewField = value;
            }
        }

        /// <remarks/>
        public string Fault_Dialog
        {
            get
            {
                return this.fault_DialogField;
            }
            set
            {
                this.fault_DialogField = value;
            }
        }

        /// <remarks/>
        public string Fault_Category
        {
            get
            {
                return this.fault_CategoryField;
            }
            set
            {
                this.fault_CategoryField = value;
            }
        }

        /// <remarks/>
        public byte ScriptSet
        {
            get
            {
                return this.scriptSetField;
            }
            set
            {
                this.scriptSetField = value;
            }
        }

        /// <remarks/>
        public string FaultText
        {
            get
            {
                return this.faultTextField;
            }
            set
            {
                this.faultTextField = value;
            }
        }

        /// <remarks/>
        public string LogDate
        {
            get
            {
                return this.logDateField;
            }
            set
            {
                this.logDateField = value;
            }
        }

        /// <remarks/>
        public object Communal_Type
        {
            get
            {
                return this.communal_TypeField;
            }
            set
            {
                this.communal_TypeField = value;
            }
        }

        /// <remarks/>
        public object Reason_Code
        {
            get
            {
                return this.reason_CodeField;
            }
            set
            {
                this.reason_CodeField = value;
            }
        }

        /// <remarks/>
        public object Reason_Text
        {
            get
            {
                return this.reason_TextField;
            }
            set
            {
                this.reason_TextField = value;
            }
        }

        /// <remarks/>
        public byte Recharge
        {
            get
            {
                return this.rechargeField;
            }
            set
            {
                this.rechargeField = value;
            }
        }

        /// <remarks/>
        public string Recharge_Code
        {
            get
            {
                return this.recharge_CodeField;
            }
            set
            {
                this.recharge_CodeField = value;
            }
        }

        /// <remarks/>
        public object Recharge_Cost
        {
            get
            {
                return this.recharge_CostField;
            }
            set
            {
                this.recharge_CostField = value;
            }
        }

        /// <remarks/>
        public object Repair_No
        {
            get
            {
                return this.repair_NoField;
            }
            set
            {
                this.repair_NoField = value;
            }
        }

        /// <remarks/>
        public byte Repair_Status
        {
            get
            {
                return this.repair_StatusField;
            }
            set
            {
                this.repair_StatusField = value;
            }
        }

        /// <remarks/>
        public string Schedule_ID
        {
            get
            {
                return this.schedule_IDField;
            }
            set
            {
                this.schedule_IDField = value;
            }
        }

        /// <remarks/>
        public object Site_Visit
        {
            get
            {
                return this.site_VisitField;
            }
            set
            {
                this.site_VisitField = value;
            }
        }

        /// <remarks/>
        public object Additional_Info
        {
            get
            {
                return this.additional_InfoField;
            }
            set
            {
                this.additional_InfoField = value;
            }
        }

        /// <remarks/>
        public object Special_Instructions
        {
            get
            {
                return this.special_InstructionsField;
            }
            set
            {
                this.special_InstructionsField = value;
            }
        }

        /// <remarks/>
        public byte Tenancy_Type
        {
            get
            {
                return this.tenancy_TypeField;
            }
            set
            {
                this.tenancy_TypeField = value;
            }
        }

        /// <remarks/>
        public object ClientID
        {
            get
            {
                return this.clientIDField;
            }
            set
            {
                this.clientIDField = value;
            }
        }

        /// <remarks/>
        public object TenantID
        {
            get
            {
                return this.tenantIDField;
            }
            set
            {
                this.tenantIDField = value;
            }
        }

        /// <remarks/>
        public object TenantText
        {
            get
            {
                return this.tenantTextField;
            }
            set
            {
                this.tenantTextField = value;
            }
        }

        /// <remarks/>
        public string UserCode
        {
            get
            {
                return this.userCodeField;
            }
            set
            {
                this.userCodeField = value;
            }
        }

        /// <remarks/>
        public string ScriptPath
        {
            get
            {
                return this.scriptPathField;
            }
            set
            {
                this.scriptPathField = value;
            }
        }

        /// <remarks/>
        public object Contractor_Code
        {
            get
            {
                return this.contractor_CodeField;
            }
            set
            {
                this.contractor_CodeField = value;
            }
        }

        /// <remarks/>
        public string Expenditure_Code
        {
            get
            {
                return this.expenditure_CodeField;
            }
            set
            {
                this.expenditure_CodeField = value;
            }
        }

        /// <remarks/>
        public object Nominal_Code
        {
            get
            {
                return this.nominal_CodeField;
            }
            set
            {
                this.nominal_CodeField = value;
            }
        }

        /// <remarks/>
        public object Job_Code
        {
            get
            {
                return this.job_CodeField;
            }
            set
            {
                this.job_CodeField = value;
            }
        }

        /// <remarks/>
        public object Location
        {
            get
            {
                return this.locationField;
            }
            set
            {
                this.locationField = value;
            }
        }

        /// <remarks/>
        public byte RepairCount
        {
            get
            {
                return this.repairCountField;
            }
            set
            {
                this.repairCountField = value;
            }
        }

        /// <remarks/>
        public byte AdviceCount
        {
            get
            {
                return this.adviceCountField;
            }
            set
            {
                this.adviceCountField = value;
            }
        }

        /// <remarks/>
        public byte CustomCount
        {
            get
            {
                return this.customCountField;
            }
            set
            {
                this.customCountField = value;
            }
        }

        /// <remarks/>
        public byte ActionCount
        {
            get
            {
                return this.actionCountField;
            }
            set
            {
                this.actionCountField = value;
            }
        }

        /// <remarks/>
        public byte SubActionCount
        {
            get
            {
                return this.subActionCountField;
            }
            set
            {
                this.subActionCountField = value;
            }
        }

        /// <remarks/>
        public byte UpdateCount
        {
            get
            {
                return this.updateCountField;
            }
            set
            {
                this.updateCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement(IsNullable = true)]
        public KeyfaxDataFaultRepair Repair
        {
            get
            {
                return this.repairField;
            }
            set
            {
                this.repairField = value;
            }
        }

        /// <remarks/>
        public KeyfaxDataFaultAdvice Advice
        {
            get
            {
                return this.adviceField;
            }
            set
            {
                this.adviceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class KeyfaxDataFaultRepair
    {
        private string completeByField;

        private byte responseDaysField;

        private string priorityField;

        private ushort priorityIdField;

        private string repairCodeField;

        private string repairCodeExField;

        private string repairCodeDescField;

        private string unitOfMeasureField;

        private byte unitOfMeasureQuantityField;

        private string contractorField;

        private string isDeletedField;

        private string isDirtyField;

        private string isNewField;

        private string nameField;

        /// <remarks/>
        public string CompleteBy
        {
            get
            {
                return this.completeByField;
            }
            set
            {
                this.completeByField = value;
            }
        }

        /// <remarks/>
        public byte ResponseDays
        {
            get
            {
                return this.responseDaysField;
            }
            set
            {
                this.responseDaysField = value;
            }
        }

        /// <remarks/>
        public string Priority
        {
            get
            {
                return this.priorityField;
            }
            set
            {
                this.priorityField = value;
            }
        }

        /// <remarks/>
        public ushort PriorityId
        {
            get
            {
                return this.priorityIdField;
            }
            set
            {
                this.priorityIdField = value;
            }
        }

        /// <remarks/>
        public string RepairCode
        {
            get
            {
                return this.repairCodeField;
            }
            set
            {
                this.repairCodeField = value;
            }
        }

        /// <remarks/>
        public string RepairCodeEx
        {
            get
            {
                return this.repairCodeExField;
            }
            set
            {
                this.repairCodeExField = value;
            }
        }

        /// <remarks/>
        public string RepairCodeDesc
        {
            get
            {
                return this.repairCodeDescField;
            }
            set
            {
                this.repairCodeDescField = value;
            }
        }

        /// <remarks/>
        public string UnitOfMeasure
        {
            get
            {
                return this.unitOfMeasureField;
            }
            set
            {
                this.unitOfMeasureField = value;
            }
        }

        /// <remarks/>
        public byte UnitOfMeasureQuantity
        {
            get
            {
                return this.unitOfMeasureQuantityField;
            }
            set
            {
                this.unitOfMeasureQuantityField = value;
            }
        }

        /// <remarks/>
        public string Contractor
        {
            get
            {
                return this.contractorField;
            }
            set
            {
                this.contractorField = value;
            }
        }

        /// <remarks/>
        public string IsDeleted
        {
            get
            {
                return this.isDeletedField;
            }
            set
            {
                this.isDeletedField = value;
            }
        }

        /// <remarks/>
        public string IsDirty
        {
            get
            {
                return this.isDirtyField;
            }
            set
            {
                this.isDirtyField = value;
            }
        }

        /// <remarks/>
        public string IsNew
        {
            get
            {
                return this.isNewField;
            }
            set
            {
                this.isNewField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class KeyfaxDataFaultAdvice
    {
        private string adviceCodeField;

        private string adviceCodeExField;

        private string adviceCodeDescField;

        private string advicePlainTextField;

        private string isDeletedField;

        private string isDirtyField;

        private string isNewField;

        private string nameField;

        /// <remarks/>
        public string AdviceCode
        {
            get
            {
                return this.adviceCodeField;
            }
            set
            {
                this.adviceCodeField = value;
            }
        }

        /// <remarks/>
        public string AdviceCodeEx
        {
            get
            {
                return this.adviceCodeExField;
            }
            set
            {
                this.adviceCodeExField = value;
            }
        }

        /// <remarks/>
        public string AdviceCodeDesc
        {
            get
            {
                return this.adviceCodeDescField;
            }
            set
            {
                this.adviceCodeDescField = value;
            }
        }

        /// <remarks/>
        public string AdvicePlainText
        {
            get
            {
                return this.advicePlainTextField;
            }
            set
            {
                this.advicePlainTextField = value;
            }
        }

        /// <remarks/>
        public string IsDeleted
        {
            get
            {
                return this.isDeletedField;
            }
            set
            {
                this.isDeletedField = value;
            }
        }

        /// <remarks/>
        public string IsDirty
        {
            get
            {
                return this.isDirtyField;
            }
            set
            {
                this.isDirtyField = value;
            }
        }

        /// <remarks/>
        public string IsNew
        {
            get
            {
                return this.isNewField;
            }
            set
            {
                this.isNewField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }
}
