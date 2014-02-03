<?xml version="1.0"?>
<resource type="view">
	<property name="name" value="tickets_browse_view" />
	<property name="version" value="2.0,Tickets,A" />
	<property name="active_flag" value="active" />
	<aspect name="content">
		<scene
			name="Default"
			width="1024"
			height="768"
			backgroundImage="image_browse_background"
			stylesheet="stylesheet_tickets"
			onEnter="tickets_browse_view_enter">

			<actor
				name="filter_tab_1"
				x="0"
				y="0"
				width="147"
				height="69"
				font="font_akzidenz_10_bold"
				text="ALL TICKETS"
				tag="all"
				style="filter_tab_style" />

			<actor
				name="filter_tab_2"
				x="147"
				y="0"
				width="146"
				height="69"
				font="font_akzidenz_10_bold"
				text="TOP PICKS"
				tag="toppicks"
				style="filter_tab_style" />

			<actor
				name="filter_tab_3"
				x="293"
				y="0"
				width="146"
				height="69"
				font="font_akzidenz_10_bold"
				text="FAMILY"
				tag="4"
				style="filter_tab_style" />

			<actor
				name="filter_tab_4"
				x="439"
				y="0"
				width="146"
				height="69"
				font="font_akzidenz_10_bold"
				text="MUSIC"
				tag="1"
				style="filter_tab_style" />

			<actor
				name="filter_tab_5"
				x="585"
				y="0"
				width="146"
				height="69"
				font="font_akzidenz_10_bold"
				text="SPORTS"
				tag="2"
				style="filter_tab_style" />

			<actor
				name="filter_tab_6"
				x="731"
				y="0"
				width="146"
				height="69"
				font="font_akzidenz_10_bold"
				text="THEATER AND ARTS"
				textRegion="36,0,73,69"
				tag="3"
				style="filter_tab_style" />

			<actor
				name="filter_tab_7"
				x="877"
				y="0"
				width="147"
				height="69"
				font="font_akzidenz_10_bold"
				text="OTHER"
				tag="5"
				style="filter_tab_style" />

			<actor
				name="left_shadow"
				x="0"
				y="69"
				image="image_left_shadow"
				optionFlags="Image" />

			<actor
				name="right_shadow"
				x="927"
				y="69"
				image="image_right_shadow"
				optionFlags="Image" />

			<actor
				name="lower_background"
				x="0"
				y="569"
				width="1024"
				height="55"
				backgroundColor="167,167,167"
				optionFlags="Background" />

			<actor
				name="page_label"
				x="493"
				y="80"
				font="font_akzidenz_11_bold"
				foregroundColor="51,51,51"
				text="${Page} of ${TotalPages}"
				optionFlags="Text" />

			<actor
				name="left_arrow"
				x="0"
				y="183"
				hitFlags="Enabled"
				onHit="tickets_browse_page_back"
				optionFlags="Image" />

			<actor
				name="right_arrow"
				x="958"
				y="183"
				hitFlags="Enabled"
				onHit="tickets_browse_page_next"
				optionFlags="Image" />

		    <actor
				name="start_over_button"
				x="8"
				y="580"
				width="115"
				height="36"
				font="font_target_12_regular"
				text="     Start Over"
				style="gray_button_style" />

			<actor
				name="start_over_arrow"
				x="20"
				y="589"
				image="image_start_over_arrow"
				optionFlags="Image" />

			<actor
				name="sort_filter_tab_1"
				x="210"
				y="580"
				width="121"
				height="33"
				font="font_akzidenz_11_bold"
				text="Soonest"
				tag="soonest"
				style="sort_filter_tab_style" />

			<actor
				name="sort_filter_tab_2"
				x="340"
				y="580"
				width="121"
				height="33"
				font="font_akzidenz_11_bold"
				text="This Weekend"
				tag="thisweekend"
				style="sort_filter_tab_style" />

			<actor
				name="sort_filter_tab_3"
				x="470"
				y="580"
				width="121"
				height="33"
				font="font_akzidenz_11_bold"
				text="Next Week"
				tag="nextweek"
				style="sort_filter_tab_style" />

			<actor
				name="sort_filter_tab_4"
				x="600"
				y="580"
				width="121"
				height="33"
				font="font_akzidenz_11_bold"
				text="Just Added"
				tag="justadded"
				style="sort_filter_tab_style" />

			<actor
				name="sort_filter_tab_5"
				x="730"
				y="580"
				width="121"
				height="33"
				font="font_akzidenz_11_bold"
				text="A-Z"
				tag="az"
				style="sort_filter_tab_style" />

			<actor
				name="redbox_logo"
				x="56"
				y="669"
				width="133"
				height="40"
				image="image_redbox_logo"
				optionFlags="Image" />

			<actor
				name="get_local_tickets1_label"
				x="262"
				y="667"
				font="font_target_medium"
				foregroundColor="229,229,229"
				text="GET TICKETS TO LOCAL EVENTS"
				optionFlags="Text" />

			<actor
				name="get_local_tickets2_label"
				x="327"
				y="711"
				font="font_target_22_regular"
				foregroundColor="229,229,229"
				text="${get_local_tickets2_label:text}"
				optionFlags="Text" />

			<actor
				name="how_it_works_button"
				x="784"
				y="660"
				width="225"
				height="74"
				cornerSize="37, 37"
				font="font_target_21_regular"
				text="HOW IT WORKS"
				style="red_button_style_3pt_redstroke" />

		</scene>
	</aspect>
</resource>
