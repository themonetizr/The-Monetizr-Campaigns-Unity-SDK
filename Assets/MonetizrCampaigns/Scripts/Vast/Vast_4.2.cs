namespace Monetizr.SDK.VAST
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VAST {
        
        private object[] itemsField;
        private string versionField;
        
        [System.Xml.Serialization.XmlElementAttribute("Ad", typeof(VASTAD))]
        [System.Xml.Serialization.XmlElementAttribute("Error", typeof(string), DataType="anyURI")]
        public object[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VASTAD {
        
        private AdDefinitionBase_type itemField;
        private string idField;
        private string sequenceField;
        private bool conditionalAdField;
        private bool conditionalAdFieldSpecified;
        private VASTADAdType adTypeField;
        private bool adTypeFieldSpecified;
        
        [System.Xml.Serialization.XmlElementAttribute("InLine", typeof(Inline_type))]
        [System.Xml.Serialization.XmlElementAttribute("Wrapper", typeof(Wrapper_type))]
        public AdDefinitionBase_type Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string sequence {
            get {
                return this.sequenceField;
            }
            set {
                this.sequenceField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool conditionalAd {
            get {
                return this.conditionalAdField;
            }
            set {
                this.conditionalAdField = value;
            }
        }
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool conditionalAdSpecified {
            get {
                return this.conditionalAdFieldSpecified;
            }
            set {
                this.conditionalAdFieldSpecified = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public VASTADAdType adType {
            get {
                return this.adTypeField;
            }
            set {
                this.adTypeField = value;
            }
        }
        
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adTypeSpecified {
            get {
                return this.adTypeFieldSpecified;
            }
            set {
                this.adTypeFieldSpecified = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Inline_type : AdDefinitionBase_type {
        
        private string adServingIdField;
        private string adTitleField;
        private Verification_type[] adVerificationsField;
        private string advertiserField;
        private Inline_typeCategory[] categoryField;
        private Creative_Inline_type[] creativesField;
        private string descriptionField;
        private string expiresField;
        private Inline_typeSurvey surveyField;
        
        public string AdServingId {
            get {
                return this.adServingIdField;
            }
            set {
                this.adServingIdField = value;
            }
        }
        
        public string AdTitle {
            get {
                return this.adTitleField;
            }
            set {
                this.adTitleField = value;
            }
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("Verification", IsNullable=false)]
        public Verification_type[] AdVerifications {
            get {
                return this.adVerificationsField;
            }
            set {
                this.adVerificationsField = value;
            }
        }
        
        public string Advertiser {
            get {
                return this.advertiserField;
            }
            set {
                this.advertiserField = value;
            }
        }
        
        [System.Xml.Serialization.XmlElementAttribute("Category")]
        public Inline_typeCategory[] Category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("Creative", IsNullable=false)]
        public Creative_Inline_type[] Creatives {
            get {
                return this.creativesField;
            }
            set {
                this.creativesField = value;
            }
        }
        
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        [System.Xml.Serialization.XmlElementAttribute(DataType="integer")]
        public string Expires {
            get {
                return this.expiresField;
            }
            set {
                this.expiresField = value;
            }
        }
        
        public Inline_typeSurvey Survey {
            get {
                return this.surveyField;
            }
            set {
                this.surveyField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Verification_type {
        
        private Verification_typeExecutableResource[] executableResourceField;
        private Verification_typeJavaScriptResource[] javaScriptResourceField;
        private TrackingEvents_Verification_typeTracking[] trackingEventsField;
        private string verificationParametersField;
        private string vendorField;
        
        [System.Xml.Serialization.XmlElementAttribute("ExecutableResource")]
        public Verification_typeExecutableResource[] ExecutableResource {
            get {
                return this.executableResourceField;
            }
            set {
                this.executableResourceField = value;
            }
        }
        
        [System.Xml.Serialization.XmlElementAttribute("JavaScriptResource")]
        public Verification_typeJavaScriptResource[] JavaScriptResource {
            get {
                return this.javaScriptResourceField;
            }
            set {
                this.javaScriptResourceField = value;
            }
        }
        
        [System.Xml.Serialization.XmlArrayItemAttribute("Tracking", IsNullable=false)]
        public TrackingEvents_Verification_typeTracking[] TrackingEvents {
            get {
                return this.trackingEventsField;
            }
            set {
                this.trackingEventsField = value;
            }
        }
        
        public string VerificationParameters {
            get {
                return this.verificationParametersField;
            }
            set {
                this.verificationParametersField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string vendor {
            get {
                return this.vendorField;
            }
            set {
                this.vendorField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Verification_typeExecutableResource {
        
        private string apiFrameworkField;
        private string typeField;
        private string valueField;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NonLinearAd_Base_type {
        
        private NonLinearAd_Base_typeNonLinearClickTracking nonLinearClickTrackingField;
        
        public NonLinearAd_Base_typeNonLinearClickTracking NonLinearClickTracking {
            get {
                return this.nonLinearClickTrackingField;
            }
            set {
                this.nonLinearClickTrackingField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class NonLinearAd_Base_typeNonLinearClickTracking {
        
        private string idField;
        private string valueField;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VideoClicks_type {
        
        private VideoClicks_typeClickTracking[] clickTrackingField;
        private VideoClicks_typeClickThrough clickThroughField;
        private VideoClicks_typeCustomClick[] customClickField;
        
        [System.Xml.Serialization.XmlElementAttribute("ClickTracking")]
        public VideoClicks_typeClickTracking[] ClickTracking {
            get {
                return this.clickTrackingField;
            }
            set {
                this.clickTrackingField = value;
            }
        }
        
        public VideoClicks_typeClickThrough ClickThrough {
            get {
                return this.clickThroughField;
            }
            set {
                this.clickThroughField = value;
            }
        }
        
        [System.Xml.Serialization.XmlElementAttribute("CustomClick")]
        public VideoClicks_typeCustomClick[] CustomClick {
            get {
                return this.customClickField;
            }
            set {
                this.customClickField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VideoClicks_typeClickTracking {
        
        private string idField;
        private string valueField;
        
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class VideoClicks_typeClickThrough {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class VideoClicks_typeCustomClick {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Linear_Inline_type))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Linear_Wrapper_type))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Base_type {
        
        private Icon_type[] iconsField;
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private string skipoffsetField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Icon", IsNullable=false)]
        public Icon_type[] Icons {
            get {
                return this.iconsField;
            }
            set {
                this.iconsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Tracking", IsNullable=false)]
        public TrackingEvents_typeTracking[] TrackingEvents {
            get {
                return this.trackingEventsField;
            }
            set {
                this.trackingEventsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string skipoffset {
            get {
                return this.skipoffsetField;
            }
            set {
                this.skipoffsetField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Icon_type : CreativeResource_type {
        
        private Icon_typeIconClicks iconClicksField;
        
        private string[] iconViewTrackingField;
        
        private string programField;
        
        private string widthField;
        
        private string heightField;
        
        private string xPositionField;
        
        private string yPositionField;
        
        private System.DateTime durationField;
        
        private bool durationFieldSpecified;
        
        private System.DateTime offsetField;
        
        private bool offsetFieldSpecified;
        
        private string apiFrameworkField;
        
        private decimal pxratioField;
        
        private bool pxratioFieldSpecified;
        
        /// <remarks/>
        public Icon_typeIconClicks IconClicks {
            get {
                return this.iconClicksField;
            }
            set {
                this.iconClicksField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IconViewTracking", DataType="anyURI")]
        public string[] IconViewTracking {
            get {
                return this.iconViewTrackingField;
            }
            set {
                this.iconViewTrackingField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string program {
            get {
                return this.programField;
            }
            set {
                this.programField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string xPosition {
            get {
                return this.xPositionField;
            }
            set {
                this.xPositionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string yPosition {
            get {
                return this.yPositionField;
            }
            set {
                this.yPositionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="time")]
        public System.DateTime duration {
            get {
                return this.durationField;
            }
            set {
                this.durationField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool durationSpecified {
            get {
                return this.durationFieldSpecified;
            }
            set {
                this.durationFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="time")]
        public System.DateTime offset {
            get {
                return this.offsetField;
            }
            set {
                this.offsetField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool offsetSpecified {
            get {
                return this.offsetFieldSpecified;
            }
            set {
                this.offsetFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal pxratio {
            get {
                return this.pxratioField;
            }
            set {
                this.pxratioField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pxratioSpecified {
            get {
                return this.pxratioFieldSpecified;
            }
            set {
                this.pxratioFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Icon_typeIconClicks {
        
        private Icon_typeIconClicksIconClickFallbackImage[] iconClickFallbackImagesField;
        
        private string iconClickThroughField;
        
        private IconClickTracking_type[] iconClickTrackingField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("IconClickFallbackImage", IsNullable=false)]
        public Icon_typeIconClicksIconClickFallbackImage[] IconClickFallbackImages {
            get {
                return this.iconClickFallbackImagesField;
            }
            set {
                this.iconClickFallbackImagesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string IconClickThrough {
            get {
                return this.iconClickThroughField;
            }
            set {
                this.iconClickThroughField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IconClickTracking")]
        public IconClickTracking_type[] IconClickTracking {
            get {
                return this.iconClickTrackingField;
            }
            set {
                this.iconClickTrackingField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Icon_typeIconClicksIconClickFallbackImage {
        
        private string altTextField;
        
        private string staticResourceField;
        
        private string heightField;
        
        private string widthField;
        
        /// <remarks/>
        public string AltText {
            get {
                return this.altTextField;
            }
            set {
                this.altTextField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string StaticResource {
            get {
                return this.staticResourceField;
            }
            set {
                this.staticResourceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class IconClickTracking_type {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CompanionAd_type))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(NonLinearAd_Inline_type))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Icon_type))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class CreativeResource_type {
        
        private HTMLResource_type[] hTMLResourceField;
        
        private string[] iFrameResourceField;
        
        private CreativeResource_typeStaticResource[] staticResourceField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HTMLResource")]
        public HTMLResource_type[] HTMLResource {
            get {
                return this.hTMLResourceField;
            }
            set {
                this.hTMLResourceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("IFrameResource", DataType="anyURI")]
        public string[] IFrameResource {
            get {
                return this.iFrameResourceField;
            }
            set {
                this.iFrameResourceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StaticResource")]
        public CreativeResource_typeStaticResource[] StaticResource {
            get {
                return this.staticResourceField;
            }
            set {
                this.staticResourceField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class HTMLResource_type {
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class CreativeResource_typeStaticResource {
        
        private string creativeTypeField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string creativeType {
            get {
                return this.creativeTypeField;
            }
            set {
                this.creativeTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class CompanionAd_type : CreativeResource_type {
        
        private AdParameters_type adParametersField;
        
        private string altTextField;
        
        private string companionClickThroughField;
        
        private CompanionAd_typeCompanionClickTracking[] companionClickTrackingField;
        
        private CreativeExtensions_typeCreativeExtension[] creativeExtensionsField;
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private string idField;
        
        private string widthField;
        
        private string heightField;
        
        private string assetWidthField;
        
        private string assetHeightField;
        
        private string expandedWidthField;
        
        private string expandedHeightField;
        
        private string apiFrameworkField;
        
        private string adSlotIdField;
        
        private decimal pxratioField;
        
        private bool pxratioFieldSpecified;
        
        private CompanionAd_typeRenderingMode renderingModeField;
        
        private bool renderingModeFieldSpecified;
        
        /// <remarks/>
        public AdParameters_type AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
            }
        }
        
        /// <remarks/>
        public string AltText {
            get {
                return this.altTextField;
            }
            set {
                this.altTextField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string CompanionClickThrough {
            get {
                return this.companionClickThroughField;
            }
            set {
                this.companionClickThroughField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CompanionClickTracking")]
        public CompanionAd_typeCompanionClickTracking[] CompanionClickTracking {
            get {
                return this.companionClickTrackingField;
            }
            set {
                this.companionClickTrackingField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("CreativeExtension", IsNullable=false)]
        public CreativeExtensions_typeCreativeExtension[] CreativeExtensions {
            get {
                return this.creativeExtensionsField;
            }
            set {
                this.creativeExtensionsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Tracking", IsNullable=false)]
        public TrackingEvents_typeTracking[] TrackingEvents {
            get {
                return this.trackingEventsField;
            }
            set {
                this.trackingEventsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string assetWidth {
            get {
                return this.assetWidthField;
            }
            set {
                this.assetWidthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string assetHeight {
            get {
                return this.assetHeightField;
            }
            set {
                this.assetHeightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string expandedWidth {
            get {
                return this.expandedWidthField;
            }
            set {
                this.expandedWidthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string expandedHeight {
            get {
                return this.expandedHeightField;
            }
            set {
                this.expandedHeightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string adSlotId {
            get {
                return this.adSlotIdField;
            }
            set {
                this.adSlotIdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal pxratio {
            get {
                return this.pxratioField;
            }
            set {
                this.pxratioField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool pxratioSpecified {
            get {
                return this.pxratioFieldSpecified;
            }
            set {
                this.pxratioFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CompanionAd_typeRenderingMode renderingMode {
            get {
                return this.renderingModeField;
            }
            set {
                this.renderingModeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool renderingModeSpecified {
            get {
                return this.renderingModeFieldSpecified;
            }
            set {
                this.renderingModeFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class AdParameters_type {
        
        private bool xmlEncodedField;
        
        private bool xmlEncodedFieldSpecified;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool xmlEncoded {
            get {
                return this.xmlEncodedField;
            }
            set {
                this.xmlEncodedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool xmlEncodedSpecified {
            get {
                return this.xmlEncodedFieldSpecified;
            }
            set {
                this.xmlEncodedFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class CompanionAd_typeCompanionClickTracking {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class CreativeExtensions_typeCreativeExtension {
        
        private System.Xml.XmlElement[] anyField;
        
        private string typeField;
        
        private System.Xml.XmlAttribute[] anyAttrField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlElement[] Any {
            get {
                return this.anyField;
            }
            set {
                this.anyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr {
            get {
                return this.anyAttrField;
            }
            set {
                this.anyAttrField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class TrackingEvents_typeTracking {
        
        //private TrackingEvents_typeTrackingEvent eventField;
        private string eventField;
        
        private string offsetField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        //public TrackingEvents_typeTrackingEvent @event {
        public string @event {
            get {
                return this.eventField;
            }
            set {
                this.eventField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string offset {
            get {
                return this.offsetField;
            }
            set {
                this.offsetField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum TrackingEvents_typeTrackingEvent {
        
        /// <remarks/>
        mute,
        
        /// <remarks/>
        unmute,
        
        /// <remarks/>
        pause,
        
        /// <remarks/>
        resume,
        
        /// <remarks/>
        rewind,
        
        /// <remarks/>
        skip,
        
        /// <remarks/>
        playerExpand,
        
        /// <remarks/>
        playerCollapse,
        
        /// <remarks/>
        loaded,
        
        /// <remarks/>
        start,
        
        /// <remarks/>
        firstQuartile,
        
        /// <remarks/>
        midpoint,
        
        /// <remarks/>
        thirdQuartile,
        
        /// <remarks/>
        complete,
        
        /// <remarks/>
        progress,
        
        /// <remarks/>
        closeLinear,
        
        /// <remarks/>
        creativeView,
        
        /// <remarks/>
        acceptInvitation,
        
        /// <remarks/>
        adExpand,
        
        /// <remarks/>
        adCollapse,
        
        /// <remarks/>
        minimize,
        
        /// <remarks/>
        close,
        
        /// <remarks/>
        overlayViewDuration,
        
        /// <remarks/>
        otherAdInteraction,
        
        /// <remarks/>
        interactiveStart,
        
        expand,
        
        collapse,
        
        fullscreen,
        
        exitFullscreen
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum CompanionAd_typeRenderingMode {
        
        /// <remarks/>
        @default,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("end-card")]
        endcard,
        
        /// <remarks/>
        concurrent,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class NonLinearAd_Inline_type : CreativeResource_type {
        
        private AdParameters_type adParametersField;
        
        private string nonLinearClickThroughField;
        
        private NonLinearAd_Inline_typeNonLinearClickTracking[] nonLinearClickTrackingField;
        
        private string idField;
        
        private string widthField;
        
        private string heightField;
        
        private string expandedWidthField;
        
        private string expandedHeightField;
        
        private bool scalableField;
        
        private bool scalableFieldSpecified;
        
        private bool maintainAspectRatioField;
        
        private bool maintainAspectRatioFieldSpecified;
        
        private System.DateTime minSuggestedDurationField;
        
        private bool minSuggestedDurationFieldSpecified;
        
        private string apiFrameworkField;
        
        /// <remarks/>
        public AdParameters_type AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string NonLinearClickThrough {
            get {
                return this.nonLinearClickThroughField;
            }
            set {
                this.nonLinearClickThroughField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NonLinearClickTracking")]
        public NonLinearAd_Inline_typeNonLinearClickTracking[] NonLinearClickTracking {
            get {
                return this.nonLinearClickTrackingField;
            }
            set {
                this.nonLinearClickTrackingField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string expandedWidth {
            get {
                return this.expandedWidthField;
            }
            set {
                this.expandedWidthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string expandedHeight {
            get {
                return this.expandedHeightField;
            }
            set {
                this.expandedHeightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool scalable {
            get {
                return this.scalableField;
            }
            set {
                this.scalableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scalableSpecified {
            get {
                return this.scalableFieldSpecified;
            }
            set {
                this.scalableFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool maintainAspectRatio {
            get {
                return this.maintainAspectRatioField;
            }
            set {
                this.maintainAspectRatioField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maintainAspectRatioSpecified {
            get {
                return this.maintainAspectRatioFieldSpecified;
            }
            set {
                this.maintainAspectRatioFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="time")]
        public System.DateTime minSuggestedDuration {
            get {
                return this.minSuggestedDurationField;
            }
            set {
                this.minSuggestedDurationField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool minSuggestedDurationSpecified {
            get {
                return this.minSuggestedDurationFieldSpecified;
            }
            set {
                this.minSuggestedDurationFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class NonLinearAd_Inline_typeNonLinearClickTracking {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_type : Linear_Base_type {
        
        private AdParameters_type adParametersField;
        
        private System.DateTime durationField;
        
        private Linear_Inline_typeMediaFiles mediaFilesField;
        
        private VideoClicks_type videoClicksField;
        
        /// <remarks/>
        public AdParameters_type AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="time")]
        public System.DateTime Duration {
            get {
                return this.durationField;
            }
            set {
                this.durationField = value;
            }
        }
        
        /// <remarks/>
        public Linear_Inline_typeMediaFiles MediaFiles {
            get {
                return this.mediaFilesField;
            }
            set {
                this.mediaFilesField = value;
            }
        }
        
        /// <remarks/>
        public VideoClicks_type VideoClicks {
            get {
                return this.videoClicksField;
            }
            set {
                this.videoClicksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_typeMediaFiles {
        
        private Linear_Inline_typeMediaFilesClosedCaptionFile[] closedCaptionFilesField;
        
        private Linear_Inline_typeMediaFilesMediaFile[] mediaFileField;
        
        private Linear_Inline_typeMediaFilesMezzanine[] mezzanineField;
        
        private Linear_Inline_typeMediaFilesInteractiveCreativeFile[] interactiveCreativeFileField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ClosedCaptionFile", IsNullable=false)]
        public Linear_Inline_typeMediaFilesClosedCaptionFile[] ClosedCaptionFiles {
            get {
                return this.closedCaptionFilesField;
            }
            set {
                this.closedCaptionFilesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MediaFile")]
        public Linear_Inline_typeMediaFilesMediaFile[] MediaFile {
            get {
                return this.mediaFileField;
            }
            set {
                this.mediaFileField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Mezzanine")]
        public Linear_Inline_typeMediaFilesMezzanine[] Mezzanine {
            get {
                return this.mezzanineField;
            }
            set {
                this.mezzanineField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InteractiveCreativeFile")]
        public Linear_Inline_typeMediaFilesInteractiveCreativeFile[] InteractiveCreativeFile {
            get {
                return this.interactiveCreativeFileField;
            }
            set {
                this.interactiveCreativeFileField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_typeMediaFilesClosedCaptionFile {
        
        private string typeField;
        
        private string languageField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string language {
            get {
                return this.languageField;
            }
            set {
                this.languageField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_typeMediaFilesMediaFile {
        
        private string idField;
        
        private Linear_Inline_typeMediaFilesMediaFileDelivery deliveryField;
        
        private string typeField;
        
        private string widthField;
        
        private string heightField;
        
        private string codecField;
        
        private string bitrateField;
        
        private string minBitrateField;
        
        private string maxBitrateField;
        
        private bool scalableField;
        
        private bool scalableFieldSpecified;
        
        private bool maintainAspectRatioField;
        
        private bool maintainAspectRatioFieldSpecified;
        
        private string fileSizeField;
        
        private string mediaTypeField;
        
        private string apiFrameworkField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Linear_Inline_typeMediaFilesMediaFileDelivery delivery {
            get {
                return this.deliveryField;
            }
            set {
                this.deliveryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string codec {
            get {
                return this.codecField;
            }
            set {
                this.codecField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string bitrate {
            get {
                return this.bitrateField;
            }
            set {
                this.bitrateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string minBitrate {
            get {
                return this.minBitrateField;
            }
            set {
                this.minBitrateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string maxBitrate {
            get {
                return this.maxBitrateField;
            }
            set {
                this.maxBitrateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool scalable {
            get {
                return this.scalableField;
            }
            set {
                this.scalableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool scalableSpecified {
            get {
                return this.scalableFieldSpecified;
            }
            set {
                this.scalableFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool maintainAspectRatio {
            get {
                return this.maintainAspectRatioField;
            }
            set {
                this.maintainAspectRatioField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool maintainAspectRatioSpecified {
            get {
                return this.maintainAspectRatioFieldSpecified;
            }
            set {
                this.maintainAspectRatioFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string fileSize {
            get {
                return this.fileSizeField;
            }
            set {
                this.fileSizeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mediaType {
            get {
                return this.mediaTypeField;
            }
            set {
                this.mediaTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum Linear_Inline_typeMediaFilesMediaFileDelivery {
        
        /// <remarks/>
        streaming,
        
        /// <remarks/>
        progressive,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_typeMediaFilesMezzanine {
        
        private string idField;
        
        private Linear_Inline_typeMediaFilesMezzanineDelivery deliveryField;
        
        private string typeField;
        
        private string widthField;
        
        private string heightField;
        
        private string codecField;
        
        private string fileSizeField;
        
        private string mediaTypeField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Linear_Inline_typeMediaFilesMezzanineDelivery delivery {
            get {
                return this.deliveryField;
            }
            set {
                this.deliveryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string width {
            get {
                return this.widthField;
            }
            set {
                this.widthField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string height {
            get {
                return this.heightField;
            }
            set {
                this.heightField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string codec {
            get {
                return this.codecField;
            }
            set {
                this.codecField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string fileSize {
            get {
                return this.fileSizeField;
            }
            set {
                this.fileSizeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mediaType {
            get {
                return this.mediaTypeField;
            }
            set {
                this.mediaTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum Linear_Inline_typeMediaFilesMezzanineDelivery {
        
        /// <remarks/>
        streaming,
        
        /// <remarks/>
        progressive,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Inline_typeMediaFilesInteractiveCreativeFile {
        
        private string typeField;
        
        private string apiFrameworkField;
        
        private bool variableDurationField;
        
        private bool variableDurationFieldSpecified;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool variableDuration {
            get {
                return this.variableDurationField;
            }
            set {
                this.variableDurationField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool variableDurationSpecified {
            get {
                return this.variableDurationFieldSpecified;
            }
            set {
                this.variableDurationFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Linear_Wrapper_type : Linear_Base_type {
        
        private VideoClicks_type videoClicksField;
        
        /// <remarks/>
        public VideoClicks_type VideoClicks {
            get {
                return this.videoClicksField;
            }
            set {
                this.videoClicksField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class CompanionAds_Collection_type {
        
        private CompanionAd_type[] companionField;
        
        private CompanionAds_Collection_typeRequired requiredField;
        
        private bool requiredFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Companion")]
        public CompanionAd_type[] Companion {
            get {
                return this.companionField;
            }
            set {
                this.companionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CompanionAds_Collection_typeRequired required {
            get {
                return this.requiredField;
            }
            set {
                this.requiredField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool requiredSpecified {
            get {
                return this.requiredFieldSpecified;
            }
            set {
                this.requiredFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum CompanionAds_Collection_typeRequired {
        
        /// <remarks/>
        all,
        
        /// <remarks/>
        any,
        
        /// <remarks/>
        none,
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Creative_Inline_type))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Creative_Wrapper_type))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Base_type {
        
        private string sequenceField;
        
        private string apiFrameworkField;
        
        private string idField;
        
        private string adIdField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="integer")]
        public string sequence {
            get {
                return this.sequenceField;
            }
            set {
                this.sequenceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string adId {
            get {
                return this.adIdField;
            }
            set {
                this.adIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Inline_type : Creative_Base_type {
        
        private CompanionAds_Collection_type companionAdsField;
        
        private CreativeExtensions_typeCreativeExtension[] creativeExtensionsField;
        
        private Linear_Inline_type linearField;
        
        private Creative_Inline_typeNonLinearAds nonLinearAdsField;
        
        private Creative_Inline_typeUniversalAdId[] universalAdIdField;
        
        /// <remarks/>
        public CompanionAds_Collection_type CompanionAds {
            get {
                return this.companionAdsField;
            }
            set {
                this.companionAdsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("CreativeExtension", IsNullable=false)]
        public CreativeExtensions_typeCreativeExtension[] CreativeExtensions {
            get {
                return this.creativeExtensionsField;
            }
            set {
                this.creativeExtensionsField = value;
            }
        }
        
        /// <remarks/>
        public Linear_Inline_type Linear {
            get {
                return this.linearField;
            }
            set {
                this.linearField = value;
            }
        }
        
        /// <remarks/>
        public Creative_Inline_typeNonLinearAds NonLinearAds {
            get {
                return this.nonLinearAdsField;
            }
            set {
                this.nonLinearAdsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UniversalAdId")]
        public Creative_Inline_typeUniversalAdId[] UniversalAdId {
            get {
                return this.universalAdIdField;
            }
            set {
                this.universalAdIdField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Inline_typeNonLinearAds {
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private NonLinearAd_Inline_type[] nonLinearField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Tracking", IsNullable=false)]
        public TrackingEvents_typeTracking[] TrackingEvents {
            get {
                return this.trackingEventsField;
            }
            set {
                this.trackingEventsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NonLinear")]
        public NonLinearAd_Inline_type[] NonLinear {
            get {
                return this.nonLinearField;
            }
            set {
                this.nonLinearField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Inline_typeUniversalAdId {
        
        private string idRegistryField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string idRegistry {
            get {
                return this.idRegistryField;
            }
            set {
                this.idRegistryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Wrapper_type : Creative_Base_type {
        
        private CompanionAds_Collection_type companionAdsField;
        
        private Linear_Wrapper_type linearField;
        
        private Creative_Wrapper_typeNonLinearAds nonLinearAdsField;
        
        /// <remarks/>
        public CompanionAds_Collection_type CompanionAds {
            get {
                return this.companionAdsField;
            }
            set {
                this.companionAdsField = value;
            }
        }
        
        /// <remarks/>
        public Linear_Wrapper_type Linear {
            get {
                return this.linearField;
            }
            set {
                this.linearField = value;
            }
        }
        
        /// <remarks/>
        public Creative_Wrapper_typeNonLinearAds NonLinearAds {
            get {
                return this.nonLinearAdsField;
            }
            set {
                this.nonLinearAdsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Creative_Wrapper_typeNonLinearAds {
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private NonLinearAd_Base_type[] nonLinearField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Tracking", IsNullable=false)]
        public TrackingEvents_typeTracking[] TrackingEvents {
            get {
                return this.trackingEventsField;
            }
            set {
                this.trackingEventsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NonLinear")]
        public NonLinearAd_Base_type[] NonLinear {
            get {
                return this.nonLinearField;
            }
            set {
                this.nonLinearField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class ViewableImpression_type {
        
        private string[] viewableField;
        
        private string[] notViewableField;
        
        private string[] viewUndeterminedField;
        
        private string idField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Viewable", DataType="anyURI")]
        public string[] Viewable {
            get {
                return this.viewableField;
            }
            set {
                this.viewableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("NotViewable", DataType="anyURI")]
        public string[] NotViewable {
            get {
                return this.notViewableField;
            }
            set {
                this.notViewableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ViewUndetermined", DataType="anyURI")]
        public string[] ViewUndetermined {
            get {
                return this.viewUndeterminedField;
            }
            set {
                this.viewUndeterminedField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Impression_type {
        
        private string idField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Inline_type))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Wrapper_type))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class AdDefinitionBase_type {
        
        private AdDefinitionBase_typeAdSystem adSystemField;
        
        private string[] errorField;
        
        private AdDefinitionBase_typeExtension[] extensionsField;
        
        private Impression_type[] impressionField;
        
        private AdDefinitionBase_typePricing pricingField;
        
        private ViewableImpression_type viewableImpressionField;
        
        /// <remarks/>
        public AdDefinitionBase_typeAdSystem AdSystem {
            get {
                return this.adSystemField;
            }
            set {
                this.adSystemField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Error", DataType="anyURI")]
        public string[] Error {
            get {
                return this.errorField;
            }
            set {
                this.errorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Extension", IsNullable=false)]
        public AdDefinitionBase_typeExtension[] Extensions {
            get {
                return this.extensionsField;
            }
            set {
                this.extensionsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Impression")]
        public Impression_type[] Impression {
            get {
                return this.impressionField;
            }
            set {
                this.impressionField = value;
            }
        }
        
        /// <remarks/>
        public AdDefinitionBase_typePricing Pricing {
            get {
                return this.pricingField;
            }
            set {
                this.pricingField = value;
            }
        }
        
        /// <remarks/>
        public ViewableImpression_type ViewableImpression {
            get {
                return this.viewableImpressionField;
            }
            set {
                this.viewableImpressionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class AdDefinitionBase_typeAdSystem {
        
        private string versionField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class AdDefinitionBase_typeExtension {
        
        private System.Xml.XmlElement[] anyField;
        
        private string typeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute()]
        public System.Xml.XmlElement[] Any {
            get {
                return this.anyField;
            }
            set {
                this.anyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class AdDefinitionBase_typePricing {
        
        private AdDefinitionBase_typePricingModel modelField;
        
        private string currencyField;
        
        private decimal valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AdDefinitionBase_typePricingModel model {
            get {
                return this.modelField;
            }
            set {
                this.modelField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string currency {
            get {
                return this.currencyField;
            }
            set {
                this.currencyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public decimal Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum AdDefinitionBase_typePricingModel {
        
        /// <remarks/>
        CPC,
        
        /// <remarks/>
        CPM,
        
        /// <remarks/>
        CPE,
        
        /// <remarks/>
        CPV,
        
        /// <remarks/>
        cpc,
        
        /// <remarks/>
        cpm,
        
        /// <remarks/>
        cpe,
        
        /// <remarks/>
        cpv,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.iab.com/VAST")]
    public partial class Wrapper_type : AdDefinitionBase_type {
        
        private Verification_type[] adVerificationsField;
        
        private Wrapper_typeBlockedAdCategories[] blockedAdCategoriesField;
        
        private Creative_Wrapper_type[] creativesField;
        
        private string vASTAdTagURIField;
        
        private bool followAdditionalWrappersField;
        
        private bool followAdditionalWrappersFieldSpecified;
        
        private bool allowMultipleAdsField;
        
        private bool allowMultipleAdsFieldSpecified;
        
        private bool fallbackOnNoAdField;
        
        private bool fallbackOnNoAdFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Verification", IsNullable=false)]
        public Verification_type[] AdVerifications {
            get {
                return this.adVerificationsField;
            }
            set {
                this.adVerificationsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BlockedAdCategories")]
        public Wrapper_typeBlockedAdCategories[] BlockedAdCategories {
            get {
                return this.blockedAdCategoriesField;
            }
            set {
                this.blockedAdCategoriesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Creative", IsNullable=false)]
        public Creative_Wrapper_type[] Creatives {
            get {
                return this.creativesField;
            }
            set {
                this.creativesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string VASTAdTagURI {
            get {
                return this.vASTAdTagURIField;
            }
            set {
                this.vASTAdTagURIField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool followAdditionalWrappers {
            get {
                return this.followAdditionalWrappersField;
            }
            set {
                this.followAdditionalWrappersField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool followAdditionalWrappersSpecified {
            get {
                return this.followAdditionalWrappersFieldSpecified;
            }
            set {
                this.followAdditionalWrappersFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool allowMultipleAds {
            get {
                return this.allowMultipleAdsField;
            }
            set {
                this.allowMultipleAdsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool allowMultipleAdsSpecified {
            get {
                return this.allowMultipleAdsFieldSpecified;
            }
            set {
                this.allowMultipleAdsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool fallbackOnNoAd {
            get {
                return this.fallbackOnNoAdField;
            }
            set {
                this.fallbackOnNoAdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fallbackOnNoAdSpecified {
            get {
                return this.fallbackOnNoAdFieldSpecified;
            }
            set {
                this.fallbackOnNoAdFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Wrapper_typeBlockedAdCategories {
        
        private string authorityField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string authority {
            get {
                return this.authorityField;
            }
            set {
                this.authorityField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Verification_typeJavaScriptResource {
        
        private string apiFrameworkField;
        
        private bool browserOptionalField;
        
        private bool browserOptionalFieldSpecified;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string apiFramework {
            get {
                return this.apiFrameworkField;
            }
            set {
                this.apiFrameworkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool browserOptional {
            get {
                return this.browserOptionalField;
            }
            set {
                this.browserOptionalField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool browserOptionalSpecified {
            get {
                return this.browserOptionalFieldSpecified;
            }
            set {
                this.browserOptionalFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class TrackingEvents_Verification_typeTracking {
        
        private string eventField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string @event {
            get {
                return this.eventField;
            }
            set {
                this.eventField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Inline_typeCategory {
        
        private string authorityField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(DataType="anyURI")]
        public string authority {
            get {
                return this.authorityField;
            }
            set {
                this.authorityField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public partial class Inline_typeSurvey {
        
        private string typeField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type {
            get {
                return this.typeField;
            }
            set {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute(DataType="anyURI")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    //[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.iab.com/VAST")]
    public enum VASTADAdType {
        
        /// <remarks/>
        video,
        
        /// <remarks/>
        audio,
        
        /// <remarks/>
        hybrid,
    }
}
