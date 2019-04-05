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
			<column name="title">------------Field Visit Form------------</column>
			<column name="url">http://qafvf/default.aspx?kioskId=${StoreNumber}</column>
		</row>
		<row>
			<column name="title">------------Install Check List------------</column>
			<column name="url">http://qaicl/default.aspx?kioskId=${StoreNumber}</column>
		</row>
	</aspect>
</resource>