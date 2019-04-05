<?xml version="1.0" encoding="utf-8" ?>
<resource type="table">
	<property name="name" value="approved_urls" />
	<property name="environment">
		<value>Production</value>
	</property>
	<aspect name="content">
		<row>
			<column name="title">------------Field Visit Form------------</column>
			<column name="url">https://fieldvisit.accessredbox.net/default.aspx?kioskId=${StoreNumber}</column>
		</row>
		<row>
			<column name="title">------------Install Check List------------</column>
			<column name="url">https://installchecklist.accessredbox.net/default.aspx?kioskId=${StoreNumber}</column>
		</row>
	</aspect>
</resource>