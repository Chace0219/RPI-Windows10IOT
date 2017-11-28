<?php
	class CControl
	{   
    	public function getRecordsParam()
		{
			$page = isset($_GET['page'])?$_GET['page']:1;

			$limit = isset($_GET['rows'])?$_GET['rows']:10; 
			$sidx = isset($_GET['sidx'])?$_GET['sidx']:"id"; 
			$sord = isset($_GET['sord'])?$_GET['sord']:""; 
			
			$filters = "";
			if(isset($_REQUEST['filters']))
				$filters = $_REQUEST['filters'];

			$array = array("page"=>$page, "limit"=>$limit, "sidx"=>$sidx, "sord"=>$sord, "filters"=>$filters);

			return $array;
		}

		public function getPostParam()
		{
			$post_array = array();
			foreach($_POST as $key=>$value){
				if($key=="oper" || $key=="id") continue;				
				$post_array[$key] = $value;
			}
			
			return $post_array;
		}

		public function getGetParam()
		{
			$post_array = array();
			foreach($_GET as $key=>$value){
				if($key=="oper" || $key=="id" || $key=="cmd" || $key=="type" || $key =="_") continue;				
				$post_array[$key] = $value;
			}
			return $post_array;
		}


	}
?>