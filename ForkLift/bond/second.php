<?php
	$obj_bond = new Bond();
	$dev_arr = $obj_bond->searchDevIds();
	$dev_select_html = "<select class='form-control' id='i_devNum' onchange='chnageDevNum(this.value);'><option value=''>- All -</option>";
	foreach( $dev_arr as $state)
	{
		$dev_select_html .= "<option value='$state'>".$state."</option>";
	}
	$dev_select_html .= "<select>";

	$user_arr = $obj_bond->searchUsers();
	$user_json = json_encode($user_arr);

?>


<style>
.form-group {margin-left:20px;}
#jqgh_jqGrid_MaturitySize, #jqgh_jqGrid_DatedDate, #jqgh_jqGrid_MaturityDate, #jqgh_jqGrid_Cusip, #jqgh_jqGrid_Description, #jqgh_jqGrid_Coupon, #jqgh_jqGrid_Campus, #jqgh_jqGrid_2Year, #jqgh_jqGrid_Type {height:40px;vertical-align:center;white-space: normal !important;}
#jqgh_jqGrid_MaturitySize span.s-ico, #jqgh_jqGrid_DatedDate span.s-ico, #jqgh_jqGrid_MaturityDate span.s-ico{ position:absolute; right:20px;top:10px;}
</style>

<div id="errordiv">
</div>
<!-- start page -->
<div id="page" >
	<!-- start content -->
	<div id="content">

		<div class="post">
			<h1 class="title">Working Hours</h1>	

			<div class="form-inline" >

				<div class="form-group">
				  <label >Dev:</label>
				  <?php echo $dev_select_html;?>
				  
				</div>
				<div class="form-group">
				  <label >Users</label>
				  <select class='form-control' id='i_userId' placeholder="Enter College">
					<option value=''>- All -</option>
				  </select>
				</div>

				<div class="form-group">
				  <label >Date:</label>
				  <input type=text id='regDate' class="form-control"/>
				  <label >Turn:</label>
				  <select id='i_turn' class="form-control">
					<option value="">- All -</option>
					<option value="1">1</option>
					<option value="2">2</option>
					<option value="3">3</option>
				  </select>
				</div>

				<button class="btn btn-default" id="i_click" onclick="search_grid();"><span class="glyphicon glyphicon-search"></span>Search</button>
			</div>

			<div style="clear:both;margin-bottom:10px;"></div>

			<table id="jqGrid"></table>
		    <div id="jqGridPager"></div>				
		</div>		
	</div>
</div>

<div>	
	<!-- end content -->
	<!-- start sidebar -->
	<div id="sidebar" style="margin-top:50px;">
	</div>
	<!-- end sidebar -->
	<div style="clear: both;">&nbsp;</div>

<!-- end page -->
<script> 
		
	var user_json = <?php echo $user_json;?>;
	//$.jgrid.defaults.width = 780;
	$.jgrid.defaults.responsive = true;
	$.jgrid.defaults.styleUI = 'Bootstrap';
	$.jgrid.useJSON = true;

        $(document).ready(function () {

			jQuery('#regDate').val("<?php echo date('Y-m-d'); ?>").datepicker({format: 'yyyy-mm-dd', orientation: "bottom auto"});

            $("#jqGrid").jqGrid({
                url:'./bond/process/bond.php?cmd=getTurn',	
                mtype: "GET",
                datatype: "json",
				styleUI : 'Bootstrap',
				loadError : function(xhr,st,err) { 
					$("#errordiv").html(xhr.responseText);
				},
				loadComplete: function(response) {
					if(response.total_duration){
						$("#jqGridPager_center").html( "<span style='margin-right:20px;'>Total working hours</span>"+response.total_duration );
					}else{
						$("#jqGridPager_center").html( "");
					}
				},
                colModel: [
					{   label : "DevNum",						
						name: 'devNumF', 
						key: true, 
						width: 50, classes:'',
						search:false, sortable: false,
						searchoptions:{sopt:['cn']}
					},
					{   label : "User",
						name: 'userIdF', 
						key: true, 
						width: 50, classes:'',
						search:false, sortable: false,
						searchoptions:{sopt:['cn']}
					},
					{   label : "Start Time",						
						name: 'startTime', 
						key: true, 
						width: 50, classes:'',
						search:false, sortable: false,
						searchoptions:{sopt:['cn']}
					},
                    {
						label: "End Time",
                        name: 'endTime',
                        width: 50,
						search:false,sortable: false,
						searchoptions:{sopt:['cn']}
                    },
					{
						label: "Working Hours",
                        name: 'Duration',
                        width: 60,
						search:false,sortable: false,
						searchoptions:{sopt:['cn']}      
                    }
                ],
				cellEdit: true,
				onSelectCell:function (id,name,val,iRow,iCol){
					/*
					if( name=="Cusip" ){
						window.open("http://emma.msrb.org/SecurityView/SecurityDetails.aspx?cusip="+val);
					}
					*/
				},
				//loadonce: true,
				viewrecords: true,
                width: 950,
                height: 450,
				//height:'auto',
                rowNum: 500000,
				sortname: 'id',				
				sortorder: 'asc',
                pager: "#jqGridPager",
				rowList: [],        // disable page size dropdown
				pgbuttons: false,     // disable page control like next, back button
				pgtext: null

            });
			// activate the toolbar searching
            
        });


		function search_grid()
		{
			var dev = $("#i_devNum").val();
			var user = $("#i_userId").val();
			var date = $("#regDate").val();
			var turn = $("#i_turn").val();	

			/*
			if(dev == "" || user == ""){
				alert("Please check your info");
				return;
			}
			*/
			
			jQuery('#jqGrid').jqGrid('setGridParam',{postData:{"dev":dev,"user":user,"date":date,"turn":turn}, page:1, search: true}); 
			jQuery('#jqGrid').trigger('reloadGrid');
		}


		function chnageDevNum(val)
		{
			var option_list = "<option value=''>- All -</option>";
			
			if( val == "" ){
				val = "All";
			}
				
			for(var i in user_json[val])
			{
				option_list += "<option value='"+user_json[val][i]+"'>"+ user_json[val][i] +"</option>";
			}

			$("#i_userId").html(option_list);
		}
		chnageDevNum("");



    </script>