<?xml version="1.0"?>
<resource type="manifest">
  <property name="name" value="manifest" />
  <property name="product_name" value="Rental Application" />
  <property name="product_version" value="3.15.0.10" />
  <property name="startup_script" value="application_startup" />
  <property name="requires">
    <value>Content Pricing,*</value>
    <value>Content Promotion,*</value>
    <value>Content Vend Screen Marketing,*</value>
    <value>Content Legal,*</value>
    <value>Library Rental Common,*</value>
    <value>Content In-Cart Run Data,*</value>
    <value>Content Email Opt-In,*</value>
    <value>Content Speech,*</value>
    <value>Content Banner Marketing,*</value>
    <value>Content Marketing Ads,*</value>
  </property>
  <property name="public_key" value="public_key" />
  <aspect name="schema">
    <resourceType name="root">
      <propertyType name="market" type="System.String" isFilter="true" score="2" label="Market" category="Filters" />
      <propertyType name="vendor" type="System.String" isFilter="true" score="2" label="Vendor" category="Filters" />
      <propertyType name="culture" type="System.String" isFilter="true" score="2" label="Culture" category="Filters" />
      <propertyType name="runtime" type="System.String" isFilter="true" score="2" label="Runtime" category="Filters" />
      <propertyType name="platform" type="System.String" isFilter="true" score="2" label="Platform" category="Filters" />
      <propertyType name="view_name" type="System.String" isFilter="true" score="2" label="View Name" category="Filters" />
      <propertyType name="environment" type="System.String" isFilter="true" score="2" label="Environment" category="Filters" />
      <propertyType name="application" type="System.String" isFilter="true" score="3" label="Application" category="Filters" />
      <propertyType name="store_number" type="System.String" isFilter="true" score="4" label="Store Number" category="Filters" />
      <propertyType name="ada_accessible" type="System.String" isFilter="true" score="3" label="ADA Accessible" category="Filters" />
      <propertyType name="name" type="System.String" label="Name" category="Common" />
      <propertyType name="version" type="System.String" label="Version" category="Common" />
      <propertyType name="created_by" type="System.String" label="Created By" category="Common" />
      <propertyType name="created_on" type="System.DateTime" label="Created On" category="Common" />
      <propertyType name="modified_by" type="System.String" label="Modified By" category="Common" />
      <propertyType name="modified_on" type="System.DateTime" label="Modified On" category="Common" />
      <propertyType name="effective_date" type="System.DateTime" label="Effective Date" category="Common" />
      <propertyType name="expiry_date" type="System.DateTime" label="Expiry Date" category="Common" />
      <propertyType name="program_name" type="System.String" label="Program Name" category="Common" />
    </resourceType>
    <resourceType name="bitmap" superType="root">
      <propertyType name="width" type="System.Int32" defaultValue="32" label="Width" category="Bitmap Properties" />
      <propertyType name="height" type="System.Int32" defaultValue="32" label="Width" category="Bitmap Properties" />
      <aspectType name="content" type="Redbox.REDS.Framework.BitmapAspectType">
        <property name="editor_type" value="bitmap" />
        <property name="label" value="bitmap" />
      </aspectType>
    </resourceType>
    <resourceType name="font" superType="root">
      <propertyType name="font_family" type="System.String" label="Font Family" category="Font Properties" />
      <propertyType name="point_size" type="System.Int32" defaultValue="10" label="Point Size" category="Font Properties" />
      <propertyType name="font_style" type="System.Drawing.FontStyle" defaultValue="Regular" label="Font Style" category="Font Properties" />
    </resourceType>
    <resourceType name="sound" superType="root" />
    <resourceType name="template" superType="root">
      <aspectType name="content" type="Redbox.REDS.Framework.TextAspectType">
        <property name="label" value="Template" />
        <property name="editor_type" value="text" />
      </aspectType>
    </resourceType>
    <resourceType name="script" superType="root">
      <aspectType name="content" type="Redbox.REDS.Framework.TextAspectType">
        <property name="editor_type" value="text" />
        <property name="label" value="Lua Script" />
        <property name="syntax_schema" value="Lua" />
      </aspectType>
    </resourceType>
    <resourceType name="halscript" superType="root">
      <aspectType name="content" type="Redbox.REDS.Framework.TextAspectType">
        <property name="editor_type" value="text" />
        <property name="label" value="HAL Script" />
        <property name="syntax_schema" value="HALScript" />
      </aspectType>
    </resourceType>
    <resourceType name="table" superType="root">
      <aspectType name="content" type="Redbox.REDS.Framework.XmlAspectType">
        <property name="editor_type" value="table" />
        <property name="label" value="Table" />
      </aspectType>
    </resourceType>
    <resourceType name="stylesheet" superType="root">
      <aspectType name="content" type="Redbox.REDS.Framework.XmlAspectType">
        <property name="editor_type" value="stylesheet" />
        <property name="label" value="Style Sheet" />
      </aspectType>
    </resourceType>
    <resourceType name="view" superType="root">
      <propertyType name="active_flag" type="System.String" label="active_flag" category="View Properties" />
      <aspectType name="content" type="Redbox.REDS.Framework.XmlAspectType">
        <property name="editor_type" value="view" />
        <property name="label" value="View" />
      </aspectType>
    </resourceType>
    <resourceType name="publickey" superType="root">
      <propertyType name="key_id" type="System.Int32" label="Key ID" category="Encryption Key Properties" />
      <aspectType name="content" type="Redbox.REDS.Framework.XmlAspectType" />
    </resourceType>
  </aspect>
</resource>