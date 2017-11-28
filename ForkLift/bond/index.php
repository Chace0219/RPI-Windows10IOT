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
			<h1 class="title">First Page</h1>	

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
		
	//$.jgrid.defaults.width = 780;
	$.jgrid.defaults.responsive = true;
	$.jgrid.defaults.styleUI = 'Bootstrap';
	$.jgrid.useJSON = true;

        $(document).ready(function () {
            $("#jqGrid").jqGrid({
                url:'./bond/process/bond.php',	
                mtype: "GET",
                datatype: "json",
				styleUI : 'Bootstrap',
				loadError : function(xhr,st,err) { 
					$("#errordiv").html(xhr.responseText);
				},
                colModel: [
					{   label : "UserID",						
						name: 'userId', 
						key: true, 
						width: 50, classes:'',
						searchoptions:{sopt:['cn']}
					},
                    {
						label: "DevNum",
                        name: 'devNum',
                        width: 50,
						searchoptions:{sopt:['cn']}
                    },
					{
						label: "Event_Time",
                        name: 'eventTime',
                        width: 60,
						searchoptions:{sopt:['cn']}      
                    },	
					{
						label: "Msg",
                        name: 'message',
                        width: 60,
						searchoptions:{sopt:['cn']},
						formatter:function(v)
						{
							return v;
						}
                    }
					,	
					{
						label: "Image",
                        name: 'Photo',
                        width: 60,
						searchoptions:{sopt:['cn']},
						search:false,
						formatter:function(v){
							if(v=="" )
								return "";
							else
								return '<img src="data:image/jpeg;base64,'+v+'" width=100 height=100/>';
						}
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
                rowNum: 50,
				sortname: 'id',				
				sortorder: 'asc',
                pager: "#jqGridPager"
            });
			// activate the toolbar searching
            $('#jqGrid').jqGrid('filterToolbar', {stringResult: true, searchOnEnter: false});
			$('#jqGrid').jqGrid('navGrid',"#jqGridPager", {                
                search: false, // show search button on the toolbar
                add: false,
                edit: false,
                del: false,
                refresh: true
            });
        });


    </script>