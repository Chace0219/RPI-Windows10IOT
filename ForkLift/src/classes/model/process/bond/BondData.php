<?php
	class BondData extends CTable
	{
		protected $tbl = TBL_REPORT_LIST;
		//protected $tbl_his = TBL_MATERIAL_HISTORY;

		public function __construct()
		{
			parent::__construct();		
		}

		public function getExeSelectQuery($table_name,$fileds,$filters,$sort_type='',$recordsParam=''){

			$query_item_array = exeSelectQuery($table_name,$fileds,$filters,$sort_type);

			$state_where_sql = "";

			if( $query_item_array["where_sql"] != "" ){
				$query_item_array["where_sql"] .= " and ".$state_where_sql.$state_where_sql;
			}else{
				$query_item_array["where_sql"] = " where 1=1 and ".$state_where_sql.$state_where_sql;
			}

			$query_item_array["where_sql"] = substr($query_item_array["where_sql"], 0, -5);			

			$query = "SELECT ".$query_item_array["fields_select"]." FROM ".$query_item_array["table_name"]." ".$query_item_array["where_sql"]." ".$query_item_array["order_by_sql"]." ".$query_item_array["limit_sql"];	
			//echo $query;
			return $query;
		}

		
		
		public function getResponseArray($result_array){

			$i = 0;
			$responce = array();
			foreach($result_array as $row)
			{		
				$row["userId"] = htmlspecialchars($row["userid"],ENT_QUOTES);
				$row["devNum"] = htmlspecialchars($row["devNum"],ENT_QUOTES);
				$row["eventTime"] = htmlspecialchars($row["eventTime"],ENT_QUOTES);

				$message = htmlspecialchars($row["message"], ENT_QUOTES);
				$message = changeNewLine($message, "<br>");
				$row["message"] = $message;
				$row["Photo"] = base64_encode($row["Photo"]);
				$responce["rows"][$i] = $row;
				$i++;
			}	
			return $responce;

		}

		public function getTurn($recordsParam)
		{
			
			$devNum = $recordsParam["devNum"];
			$userid = $recordsParam["userid"];

			//if( $devNum == "" )
			//	return array2json(array());

			$date = $recordsParam["date"];
			$turn = $recordsParam["turn"];


			$fromWhere_where = " 1=1 ";
			if( $devNum !="" )
				$fromWhere_where .= " and devNum='".$devNum."' ";
			if( $userid !="" )
				$fromWhere_where .= " and userid='".$userid."' ";

			$next_date = date('Y-m-d', strtotime( "$date + 1 day" ));

			
			$turn_array["0"] = "eventTime>= '".$date." 06:01:00' and eventTime< '".$next_date." 06:01:00'";

			$turn_array["1"] = "eventTime>= '".$date." 06:01:00' and eventTime< '".$date." 14:01:00'";
			$turn_array["2"] = "eventTime>= '".$date." 14:01:00' and eventTime< '".$date." 22:01:00'";
			$turn_array["3"] = "eventTime>= '".$date." 22:01:00' and eventTime< '".$next_date." 06:01:00'";

			if( $turn!="" )
				$fromWhere_where .= " and ".$turn_array[$turn];
			else{
				$fromWhere_where .= " and ".$turn_array[0];
			}

			$fromWhere_where .= " and message like '%Ended%'";


			
			$sql = "select * from ".TBL_REPORT_LIST." where ".$fromWhere_where." order by eventTime asc ";

			$res = $this->db_query( $sql ) or die( $sql.'->Error' );
			$nums = mysqli_num_rows($res);

			$k = 0;

			$total_seconds = 0;
			if($nums > 0){
				
				while($row = mysqli_fetch_assoc($res)){		
						$array = array();

						$array["devNumF"] = $row["devNum"];
						$array["userIdF"] = $row["userid"];
						
						$segs = explode("#", $row["message"]);						
						
						//$segs[3] = changeNewLine($segs[3], "");						
						$seconds = isset($segs[3])?$segs[3]:"";

						$array["startTime"] = isset($segs[1])?$segs[1]:"?";
						$array["endTime"] = isset($segs[2])?$segs[2]:"?";
						$array["Duration"] = date('H:i:s', mktime(0, 0, $seconds));

						$total_seconds += $seconds;

						$responce["rows"][$k]= $array;
						
						
						$k++;					
					
				}
			}

			$total_duration = date('H:i:s', mktime(0, 0, $total_seconds));
			
			$count = $k;
			$responce["page"] = $recordsParam["page"];
			$responce["total"] = ceil($count/$recordsParam["limit"]);
			$responce["records"] = $count;

			$responce["total_duration"] = $total_duration;


			return array2json($responce);

		}

		
		public function searchDevIds()
		{
			$sql = "select devNum from ".TBL_RPI_LIST." order by devNum asc";				
			$res = $this->db_query($sql);
			$state_arr = array();
			while($row = mysqli_fetch_assoc($res))
			{
				$state_arr[] = $row["devNum"];
			}
			return $state_arr;
		}

		public function searchUsers()
		{
			$sql = "select * from ".TBL_SETTING_LIST." order by devNum asc";				
			$res = $this->db_query($sql);
			$state_arr = array();
			$state_arr["All"] = array();
			while($row = mysqli_fetch_assoc($res))
			{
				if( !isset($state_arr[$row["devNum"]]) )
					$state_arr[$row["devNum"]] = array();

				if( !in_array( $row["userid"], $state_arr[$row["devNum"]] ))
					$state_arr[$row["devNum"]][] = $row["userid"];
				
				if( !in_array( $row["userid"], $state_arr["All"] ))
					$state_arr["All"][] = $row["userid"];
			}
			return $state_arr;
		}


	}
?>