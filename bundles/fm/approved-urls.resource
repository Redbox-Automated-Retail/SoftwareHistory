<?xml version="1.0" encoding="utf-8" ?>
<resource type="table">
	<property name="name" value="approved_urls" />
	<property name="environment">
		<value>Production</value>
	</property>
	<aspect name="content">
		<row>
			<column name="title">---------Merchandising Visit Checklist---------</column>
			<column name="url">https://fieldvisit.accessredbox.net/MerchandisingVisitChecklist.aspx?kioskId=${FMA:StoreNumber}&#38;userId=${FMA:UserName}</column>
		</row>		
		<row>
			<column name="title">-----------------Kiosk Survey------------------</column>
			<column name="url">https://kiosksurvey.accessredbox.net/default.aspx?kioskId=${FMA:StoreNumber}</column>
		</row>
    <row>
      <column name="title">------------Kiosk Accessibility Hours------------</column>
      <column name="url">https://fieldvisit.accessredbox.net/accessiblehours/default.aspx?kioskId=${FMA:StoreNumber}</column>
    </row>
  </aspect>
</resource>


