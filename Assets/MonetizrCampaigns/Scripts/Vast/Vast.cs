
/*
namespace Vast
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
    public partial class VAST {
        
        private VASTAD[] adField;
        
        private string versionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Ad")]
        public VASTAD[] Ad {
            get {
                return this.adField;
            }
            set {
                this.adField = value;
            }
        }
        
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
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTAD {
        
        private object itemField;
        
        private string idField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("InLine", typeof(VASTADInLine))]
        [System.Xml.Serialization.XmlElementAttribute("Wrapper", typeof(VASTADWrapper))]
        public object Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLine {
        
        private AdSystem_type adSystemField;
        
        private string adTitleField;
        
        private string descriptionField;
        
        private string surveyField;
        
        private string errorField;
        
        private Impression_type[] impressionField;
        
        private VASTADInLineCreative[] creativesField;
        
        private object[] extensionsField;
        
        /// <remarks/>
        public AdSystem_type AdSystem {
            get {
                return this.adSystemField;
            }
            set {
                this.adSystemField = value;
            }
        }
        
        /// <remarks/>
        public string AdTitle {
            get {
                return this.adTitleField;
            }
            set {
                this.adTitleField = value;
            }
        }
        
        /// <remarks/>
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string Survey {
            get {
                return this.surveyField;
            }
            set {
                this.surveyField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string Error {
            get {
                return this.errorField;
            }
            set {
                this.errorField = value;
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
        [System.Xml.Serialization.XmlArrayItemAttribute("Creative", IsNullable=false)]
        public VASTADInLineCreative[] Creatives {
            get {
                return this.creativesField;
            }
            set {
                this.creativesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Extension", IsNullable=false)]
        public object[] Extensions {
            get {
                return this.extensionsField;
            }
            set {
                this.extensionsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class AdSystem_type {
        
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
    public partial class NonLinear_type {
        
        private object itemField;
        
        private ItemChoiceType1 itemElementNameField;
        
        private string nonLinearClickThroughField;
        
        private string adParametersField;
        
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
        [System.Xml.Serialization.XmlElementAttribute("HTMLResource", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("IFrameResource", typeof(string), DataType="anyURI")]
        [System.Xml.Serialization.XmlElementAttribute("StaticResource", typeof(NonLinear_typeStaticResource))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemElementName")]
        public object Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemChoiceType1 ItemElementName {
            get {
                return this.itemElementNameField;
            }
            set {
                this.itemElementNameField = value;
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
        public string AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class NonLinear_typeStaticResource {
        
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
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public enum ItemChoiceType1 {
        
        /// <remarks/>
        HTMLResource,
        
        /// <remarks/>
        IFrameResource,
        
        /// <remarks/>
        StaticResource,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class Companion_type {
        
        private object itemField;
        
        private ItemChoiceType itemElementNameField;
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private string companionClickThroughField;
        
        private string altTextField;
        
        private string adParametersField;
        
        private string idField;
        
        private string widthField;
        
        private string heightField;
        
        private string expandedWidthField;
        
        private string expandedHeightField;
        
        private string apiFrameworkField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("HTMLResource", typeof(string))]
        [System.Xml.Serialization.XmlElementAttribute("IFrameResource", typeof(string), DataType="anyURI")]
        [System.Xml.Serialization.XmlElementAttribute("StaticResource", typeof(Companion_typeStaticResource))]
        [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemElementName")]
        public object Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public ItemChoiceType ItemElementName {
            get {
                return this.itemElementNameField;
            }
            set {
                this.itemElementNameField = value;
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
        public string AltText {
            get {
                return this.altTextField;
            }
            set {
                this.altTextField = value;
            }
        }
        
        /// <remarks/>
        public string AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class Companion_typeStaticResource {
        
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
    [System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public enum ItemChoiceType {
        
        /// <remarks/>
        HTMLResource,
        
        /// <remarks/>
        IFrameResource,
        
        /// <remarks/>
        StaticResource,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class TrackingEvents_typeTracking {
        
        private TrackingEvents_typeTrackingEvent eventField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TrackingEvents_typeTrackingEvent @event {
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public enum TrackingEvents_typeTrackingEvent {
        
        /// <remarks/>
        creativeView,
        
        /// <remarks/>
        start,
        
        /// <remarks/>
        midpoint,
        
        /// <remarks/>
        firstQuartile,
        
        /// <remarks/>
        thirdQuartile,
        
        /// <remarks/>
        complete,
        
        /// <remarks/>
        mute,
        
        /// <remarks/>
        unmute,
        
        /// <remarks/>
        pause,
        
        /// <remarks/>
        rewind,
        
        /// <remarks/>
        resume,
        
        /// <remarks/>
        fullscreen,
        
        /// <remarks/>
        expand,
        
        /// <remarks/>
        collapse,
        
        /// <remarks/>
        acceptInvitation,
        
        /// <remarks/>
        close,

        skip
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class VideoClicks_type {
        
        private VideoClicks_typeClickThrough clickThroughField;
        
        private VideoClicks_typeClickTracking[] clickTrackingField;
        
        private VideoClicks_typeCustomClick[] customClickField;
        
        /// <remarks/>
        public VideoClicks_typeClickThrough ClickThrough {
            get {
                return this.clickThroughField;
            }
            set {
                this.clickThroughField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ClickTracking")]
        public VideoClicks_typeClickTracking[] ClickTracking {
            get {
                return this.clickTrackingField;
            }
            set {
                this.clickTrackingField = value;
            }
        }
        
        /// <remarks/>
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
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VideoClicks_typeClickTracking {
        
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLineCreative {
        
        private object itemField;
        
        private string idField;
        
        private string sequenceField;
        
        private string adIDField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CompanionAds", typeof(VASTADInLineCreativeCompanionAds))]
        [System.Xml.Serialization.XmlElementAttribute("Linear", typeof(VASTADInLineCreativeLinear))]
        [System.Xml.Serialization.XmlElementAttribute("NonLinearAds", typeof(VASTADInLineCreativeNonLinearAds))]
        public object Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
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
        public string AdID {
            get {
                return this.adIDField;
            }
            set {
                this.adIDField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLineCreativeCompanionAds {
        
        private Companion_type[] companionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Companion")]
        public Companion_type[] Companion {
            get {
                return this.companionField;
            }
            set {
                this.companionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLineCreativeLinear {
        
        private System.DateTime durationField;
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private string adParametersField;
        
        private VideoClicks_type videoClicksField;
        
        private VASTADInLineCreativeLinearMediaFile[] mediaFilesField;
        
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
        public string AdParameters {
            get {
                return this.adParametersField;
            }
            set {
                this.adParametersField = value;
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
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("MediaFile", IsNullable=false)]
        public VASTADInLineCreativeLinearMediaFile[] MediaFiles {
            get {
                return this.mediaFilesField;
            }
            set {
                this.mediaFilesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLineCreativeLinearMediaFile {
        
        private string idField;
        
        private VASTADInLineCreativeLinearMediaFileDelivery deliveryField;
        
        private string typeField;
        
        private string bitrateField;
        
        private string widthField;
        
        private string heightField;
        
        private bool scalableField;
        
        private bool scalableFieldSpecified;
        
        private bool maintainAspectRatioField;
        
        private bool maintainAspectRatioFieldSpecified;
        
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
        public VASTADInLineCreativeLinearMediaFileDelivery delivery {
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public enum VASTADInLineCreativeLinearMediaFileDelivery {
        
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADInLineCreativeNonLinearAds {
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private NonLinear_type[] nonLinearField;
        
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
        public NonLinear_type[] NonLinear {
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapper {
        
        private AdSystem_type adSystemField;
        
        private string vASTAdTagURIField;
        
        private string errorField;
        
        private string[] impressionField;
        
        private VASTADWrapperCreative[] creativesField;
        
        private object[] extensionsField;
        
        /// <remarks/>
        public AdSystem_type AdSystem {
            get {
                return this.adSystemField;
            }
            set {
                this.adSystemField = value;
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
        [System.Xml.Serialization.XmlElementAttribute(DataType="anyURI")]
        public string Error {
            get {
                return this.errorField;
            }
            set {
                this.errorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Impression", DataType="anyURI")]
        public string[] Impression {
            get {
                return this.impressionField;
            }
            set {
                this.impressionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Creative", IsNullable=false)]
        public VASTADWrapperCreative[] Creatives {
            get {
                return this.creativesField;
            }
            set {
                this.creativesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Extension", IsNullable=false)]
        public object[] Extensions {
            get {
                return this.extensionsField;
            }
            set {
                this.extensionsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapperCreative {
        
        private object itemField;
        
        private string idField;
        
        private string sequenceField;
        
        private string adIDField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("CompanionAds", typeof(VASTADWrapperCreativeCompanionAds))]
        [System.Xml.Serialization.XmlElementAttribute("Linear", typeof(VASTADWrapperCreativeLinear))]
        [System.Xml.Serialization.XmlElementAttribute("NonLinearAds", typeof(VASTADWrapperCreativeNonLinearAds))]
        public object Item {
            get {
                return this.itemField;
            }
            set {
                this.itemField = value;
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
        public string AdID {
            get {
                return this.adIDField;
            }
            set {
                this.adIDField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapperCreativeCompanionAds {
        
        private Companion_type[] companionField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Companion")]
        public Companion_type[] Companion {
            get {
                return this.companionField;
            }
            set {
                this.companionField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "0.0.0.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapperCreativeLinear {
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private VASTADWrapperCreativeLinearClickTracking[] videoClicksField;
        
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
        [System.Xml.Serialization.XmlArrayItemAttribute("ClickTracking", IsNullable=false)]
        public VASTADWrapperCreativeLinearClickTracking[] VideoClicks {
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapperCreativeLinearClickTracking {
        
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class VASTADWrapperCreativeNonLinearAds {
        
        private TrackingEvents_typeTracking[] trackingEventsField;
        
        private NonLinear_type[] nonLinearField;
        
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
        public NonLinear_type[] NonLinear {
            get {
                return this.nonLinearField;
            }
            set {
                this.nonLinearField = value;
            }
        }
    }
}
*/