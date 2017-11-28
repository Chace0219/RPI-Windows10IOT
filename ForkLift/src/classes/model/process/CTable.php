<?php
	class CTable
	{   
        public $_db = null;

        
        public function __construct()
        {
			global  $_db;
			$this->_db = $_db;
        }

		public function db_query($query)
		{
			mysqli_query($this->_db, "set names utf8");		
			$result = mysqli_query($this->_db, $query);
			return $result;
		}

		public function chkTblExist($tablename)
		{
			$result = mysql_list_tables( DBNAME );
			$num_rows = mysql_num_rows( $result );

			$exists_table = false;
			for( $i=0; $i<$num_rows; $i++ ){
				$tb_name = mysql_tablename($result, $i);
				if( $tablename == $tb_name ) {
					$exists_table = true;
					continue;
				}
			}
			return $exists_table;

		}


		public function insertRecord($items)
		{
		$insert_key = "(";
			$insert_value = "(";
			foreach($items as $key => $value)
			{
				$insert_key .= "`$key`,";
				$insert_value .= "'".addslashes(trim($value))."',";
			}
			$insert_key = substr($insert_key, 0, -1).")";
			$insert_value = substr($insert_value, 0, -1).")";
			$sql = "insert into ".$this->tbl." $insert_key values $insert_value";
			//$this->db_query($sql) or die("INSERTERR: ".$sql);
			$this->db_query($sql);
			//echo mysql_error()."---".$sql;
			return mysql_errno();
		}
		public function isExistRecordData($items)
		{
			$where_sql = " where ";
			foreach($items as $key => $value)
			{
				$where_sql .= " `$key`='$value' and";
			}
			$where_sql = substr($where_sql, 0, -3);
						
			$sql = "select count(*) from ".$this->tbl.$where_sql;
			
			$result = $this->db_query($sql);
			return mysql_result($result,0,0);
		}
		public function delRecord($id)
		{
			$sql = "delete from ".$this->tbl." where id='".$id."'";
			$res = $this->db_query($sql);
		}

		public function updateRecord($id, $items)
		{
			$update_where = "";

			foreach($items as $key => $value)
			{
				$update_where .= "`$key` = '".addslashes(trim($value))."',";
			}
			$update_where = substr($update_where, 0, -1);
			$sql = "update ".$this->tbl." set ".$update_where." where `id`='$id'";
			$this->db_query($sql);
			return mysql_errno();
		}		
		
		
		public function getFilters($filters)
		{
			return $filters;
		}
		
		public function getResponseArray($result_array){
			
			$i = 0;
			$responce = array();
			foreach($result_array as $row)
			{
				$responce["rows"][$i] = $row;
				/*
				$responce["rows"][$i]["id"] = $row["id"];
				$array=array();
				
				foreach($row as $v){
					$array[] = $v;
				}
				$responce["rows"][$i]["cell"]=$array;
				*/
				$i++;
			}	
			return $responce;

		}

		public function getExeSelectQuery($table_name,$fileds,$filters,$sort_type='',$recordsParam=''){
			$query_item_array = exeSelectQuery($table_name,$fileds,$filters,$sort_type);

			$query = "SELECT ".$query_item_array["fields_select"]." FROM ".$query_item_array["table_name"]." ".$query_item_array["where_sql"]." ".$query_item_array["order_by_sql"]." ".$query_item_array["limit_sql"];	

			//echo $query;
			return $query;
		}
		
		public function getRecords($recordsParam)
		{
			$page = $recordsParam["page"];
			$limit = $recordsParam["limit"];
			$sidx = $recordsParam["sidx"];
			$sord = $recordsParam["sord"];
			$filters = $recordsParam["filters"];

			$table_name = $this->tbl;
			$filters = $this->getFilters($filters);

			$fields = array("count(*) as count");
			$query = $this->getExeSelectQuery($table_name,$fields,$filters,$sort_type='',$recordsParam,"count");

			$result_array = getResult($query);


			if( count($result_array) > 0 ) { 
				$count = $result_array[0]["count"];		
				$total_pages = ceil($count/$limit); 
			} else { 
				$count=0;
				$total_pages = 0; 
			} 


			if ($page > $total_pages) $page=$total_pages;
			$start = $limit*$page - $limit;
			if($start <0) $start = 0; 		

			$sort_type["sidx"] = $sidx;
			$sort_type["sord"] = $sord;
			$sort_type["limit_start"] = $start;
			$sort_type["limit_count"] = $limit;
			
			$query = $this->getExeSelectQuery($table_name,"",$filters,$sort_type,$recordsParam,"");

			$result_array = getResult($query);
			
			$responce["records"] = $count;
			$responce["page"] = $page;
			$responce["total"] = $total_pages;
			
			$i=0;
			
			$returned_responce = $this->getResponseArray($result_array);
			$responce["rows"] = isset($returned_responce["rows"])?$returned_responce["rows"]:array();

			
			return array2json($responce);			
		}
		
	}
?>