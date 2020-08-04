<?xml version="1.0" encoding="utf-8" ?>
<resource type="table">
	<property name="name" value="approved_urls" />
	<property name="environment">
		<value>QA</value>
		<value>Development</value>
		<value>Integration</value>
	</property>
	<aspect name="content">
    <row>
      <column name="title">---------Merchandising Visit Checklist(QA)-----</column>
      <column name="url">http://qafvf/MerchandisingVisitChecklist.aspx?kioskId=${FMA:StoreNumber}&#38;userId=${FMA:UserName}</column>
    </row>
    <row>
      <column name="title">------------Kiosk Accessibility Hours(QA)------------</column>
      <column name="url">http://qafvf/accessiblehours/default.aspx?kioskId=${FMA:StoreNumber}</column>
    </row>
	</aspect>
</resource>
