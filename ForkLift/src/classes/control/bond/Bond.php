<?php
	class Bond extends CControl
	{
		private $o_materialdata;

		public function __construct()
		{
			$this->o_bonddata = new BondData;
		}

		public function getRecordsParam()
		{
			$page = (isset($_GET['page']) && $_GET['page'] !="")?$_GET['page']:1;
			$limit = (isset($_GET['rows']) && $_GET['rows'] !="")?$_GET['rows']:10; 
			$sidx = (isset($_GET['sidx']) && $_GET['sidx'] !="")?$_GET['sidx']:"id"; 
			$sord = (isset($_GET['sord']) && $_GET['sord'] !="")?$_GET['sord']:""; 
			
			$filters = "";
			if(isset($_REQUEST['filters']))
				$filters = $_REQUEST['filters'];
			//$registerDate = isset($_GET['registerDate'])?$_GET['registerDate']:""; 

			$array = array("page"=>$page, "limit"=>$limit, "sidx"=>$sidx, "sord"=>$sord, "filters"=>$filters);
			
			return $array;
		}
	
		public function getRecords()
		{

			$recordsParam = $this->getRecordsParam();// array [page, limit, sidx, sord, filters]
			
						/*
			if( $recordsParam["state"] = "" || $recordsParam["college"] == ""  ){
				return "[]";
			}else
				return $this->o_bonddata->getRecords($recordsParam);
				*/
			return $this->o_bonddata->getRecords($recordsParam);
		}

		public function getTurn()
		{
			$recordsParam = $this->getRecordsParam();// array [page, limit, sidx, sord, filters]
			$recordsParam["devNum"] = (isset($_GET['dev']) && $_GET['dev'] !="")?$_GET['dev']:""; 
			$recordsParam["userid"] = (isset($_GET['user']) && $_GET['user'] !="")?$_GET['user']:""; 
			$recordsParam["date"] = (isset($_GET['date']) && $_GET['date'] !="")?$_GET['date']:date("Y-m-d"); 
			$recordsParam["turn"] = (isset($_GET['turn']) && $_GET['turn'] !="")?$_GET['turn']:""; 

			return $this->o_bonddata->getTurn($recordsParam);
		}

		public function searchDevIds()
		{
			return $this->o_bonddata->searchDevIds();
		}

		public function searchUsers()
		{
			return $this->o_bonddata->searchUsers();
		}

		
	}
?>