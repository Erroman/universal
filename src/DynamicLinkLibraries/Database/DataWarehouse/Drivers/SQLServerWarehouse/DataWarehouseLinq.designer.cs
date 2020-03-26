﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SQLServerWarehouse
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="AstronomyExpress")]
	public partial class DataWarehouseLinqDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertBinaryTable(BinaryTable instance);
    partial void UpdateBinaryTable(BinaryTable instance);
    partial void DeleteBinaryTable(BinaryTable instance);
    partial void InsertBinaryTree(BinaryTree instance);
    partial void UpdateBinaryTree(BinaryTree instance);
    partial void DeleteBinaryTree(BinaryTree instance);
    #endregion
		
		public DataWarehouseLinqDataContext() : 
				base(global::SQLServerWarehouse.Properties.Settings.Default.AstronomyExpressConnectionString2, mappingSource)
		{
			OnCreated();
		}
		
		public DataWarehouseLinqDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataWarehouseLinqDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataWarehouseLinqDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public DataWarehouseLinqDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<BinaryTable> BinaryTables
		{
			get
			{
				return this.GetTable<BinaryTable>();
			}
		}
		
		public System.Data.Linq.Table<BinaryTree> BinaryTrees
		{
			get
			{
				return this.GetTable<BinaryTree>();
			}
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.DeleteBinary")]
		public int DeleteBinary([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.DeleteBinaryTree")]
		public int DeleteBinaryTree([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.SelectBinaryTree")]
		public ISingleResult<SelectBinaryTreeResult> SelectBinaryTree()
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())));
			return ((ISingleResult<SelectBinaryTreeResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.InsertBinary")]
		public ISingleResult<InsertBinaryResult> InsertBinary([global::System.Data.Linq.Mapping.ParameterAttribute(Name="ParentId", DbType="UniqueIdentifier")] System.Nullable<System.Guid> parentId, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Name", DbType="NVarChar(50)")] string name, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Description", DbType="NVarChar(300)")] string description, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Data", DbType="Image")] System.Data.Linq.Binary data, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Length", DbType="Int")] System.Nullable<int> length, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Ext", DbType="Char(10)")] string ext)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), parentId, name, description, data, length, ext);
			return ((ISingleResult<InsertBinaryResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.InsertBinaryInterface", IsComposable=true)]
		public object InsertBinaryInterface([global::System.Data.Linq.Mapping.ParameterAttribute(Name="ParentId", DbType="UniqueIdentifier")] System.Nullable<System.Guid> parentId, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Name", DbType="NVarChar(50)")] string name, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Description", DbType="NVarChar(300)")] string description, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Data", DbType="Image")] System.Data.Linq.Binary data, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Length", DbType="Int")] System.Nullable<int> length, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Ext", DbType="Char(10)")] string ext, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="InterfaceName", DbType="VarChar(100)")] string interfaceName)
		{
			return ((object)(this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), parentId, name, description, data, length, ext, interfaceName).ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.InsertBinaryNode")]
		public ISingleResult<InsertBinaryNodeResult> InsertBinaryNode([global::System.Data.Linq.Mapping.ParameterAttribute(Name="ParentId", DbType="UniqueIdentifier")] System.Nullable<System.Guid> parentId, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="NAME", DbType="NVarChar(50)")] string nAME, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Description", DbType="NVarChar(300)")] string description, [global::System.Data.Linq.Mapping.ParameterAttribute(DbType="Char(10)")] string ext)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), parentId, nAME, description, ext);
			return ((ISingleResult<InsertBinaryNodeResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.SelectBinary")]
		public ISingleResult<SelectBinaryResult> SelectBinary([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id);
			return ((ISingleResult<SelectBinaryResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.SelectBinaryContents")]
		public ISingleResult<SelectBinaryContentsResult> SelectBinaryContents([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Ext", DbType="VarChar(10)")] string ext)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), ext);
			return ((ISingleResult<SelectBinaryContentsResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.SelectBinaryTable")]
		public ISingleResult<SelectBinaryTableResult> SelectBinaryTable()
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())));
			return ((ISingleResult<SelectBinaryTableResult>)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateBinaryData")]
		public int UpdateBinaryData([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Data", DbType="Image")] System.Data.Linq.Binary data)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, data);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateBinaryTreeName")]
		public int UpdateBinaryTreeName([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Name", DbType="NVarChar(50)")] string name)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, name);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateBinaryTableDescription")]
		public int UpdateBinaryTableDescription([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Description", DbType="NVarChar(300)")] string description)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, description);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateBinaryTableName")]
		public int UpdateBinaryTableName([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Name", DbType="NVarChar(50)")] string name)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, name);
			return ((int)(result.ReturnValue));
		}
		
		[global::System.Data.Linq.Mapping.FunctionAttribute(Name="dbo.UpdateBinaryTreeDescription")]
		public int UpdateBinaryTreeDescription([global::System.Data.Linq.Mapping.ParameterAttribute(Name="Id", DbType="UniqueIdentifier")] System.Nullable<System.Guid> id, [global::System.Data.Linq.Mapping.ParameterAttribute(Name="Description", DbType="NVarChar(300)")] string description)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, description);
			return ((int)(result.ReturnValue));
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.BinaryTable")]
	public partial class BinaryTable : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _Id;
		
		private System.Guid _ParentId;
		
		private string _Name;
		
		private string _Description;
		
		private System.Data.Linq.Binary _Data;
		
		private string _Length;
		
		private string _Ext;
		
		private EntityRef<BinaryTree> _BinaryTree;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(System.Guid value);
    partial void OnIdChanged();
    partial void OnParentIdChanging(System.Guid value);
    partial void OnParentIdChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnDescriptionChanging(string value);
    partial void OnDescriptionChanged();
    partial void OnDataChanging(System.Data.Linq.Binary value);
    partial void OnDataChanged();
    partial void OnLengthChanging(string value);
    partial void OnLengthChanged();
    partial void OnExtChanging(string value);
    partial void OnExtChanged();
    #endregion
		
		public BinaryTable()
		{
			this._BinaryTree = default(EntityRef<BinaryTree>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ParentId
		{
			get
			{
				return this._ParentId;
			}
			set
			{
				if ((this._ParentId != value))
				{
					if (this._BinaryTree.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnParentIdChanging(value);
					this.SendPropertyChanging();
					this._ParentId = value;
					this.SendPropertyChanged("ParentId");
					this.OnParentIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(300) NOT NULL", CanBeNull=false)]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Data", DbType="Image NOT NULL", CanBeNull=false, UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if ((this._Data != value))
				{
					this.OnDataChanging(value);
					this.SendPropertyChanging();
					this._Data = value;
					this.SendPropertyChanged("Data");
					this.OnDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Length", DbType="NVarChar(30) NOT NULL", CanBeNull=false)]
		public string Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				if ((this._Length != value))
				{
					this.OnLengthChanging(value);
					this.SendPropertyChanging();
					this._Length = value;
					this.SendPropertyChanged("Length");
					this.OnLengthChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Ext", DbType="VarChar(10) NOT NULL", CanBeNull=false)]
		public string Ext
		{
			get
			{
				return this._Ext;
			}
			set
			{
				if ((this._Ext != value))
				{
					this.OnExtChanging(value);
					this.SendPropertyChanging();
					this._Ext = value;
					this.SendPropertyChanged("Ext");
					this.OnExtChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="BinaryTree_BinaryTable", Storage="_BinaryTree", ThisKey="ParentId", OtherKey="Id", IsForeignKey=true)]
		public BinaryTree BinaryTree
		{
			get
			{
				return this._BinaryTree.Entity;
			}
			set
			{
				BinaryTree previousValue = this._BinaryTree.Entity;
				if (((previousValue != value) 
							|| (this._BinaryTree.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._BinaryTree.Entity = null;
						previousValue.BinaryTables.Remove(this);
					}
					this._BinaryTree.Entity = value;
					if ((value != null))
					{
						value.BinaryTables.Add(this);
						this._ParentId = value.Id;
					}
					else
					{
						this._ParentId = default(System.Guid);
					}
					this.SendPropertyChanged("BinaryTree");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.BinaryTree")]
	public partial class BinaryTree : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _Id;
		
		private System.Guid _ParentId;
		
		private string _Name;
		
		private string _Description;
		
		private string _ext;
		
		private EntitySet<BinaryTable> _BinaryTables;
		
		private EntitySet<BinaryTree> _BinaryTrees;
		
		private EntityRef<BinaryTree> _BinaryTree1;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnIdChanging(System.Guid value);
    partial void OnIdChanged();
    partial void OnParentIdChanging(System.Guid value);
    partial void OnParentIdChanged();
    partial void OnNameChanging(string value);
    partial void OnNameChanged();
    partial void OnDescriptionChanging(string value);
    partial void OnDescriptionChanged();
    partial void OnextChanging(string value);
    partial void OnextChanged();
    #endregion
		
		public BinaryTree()
		{
			this._BinaryTables = new EntitySet<BinaryTable>(new Action<BinaryTable>(this.attach_BinaryTables), new Action<BinaryTable>(this.detach_BinaryTables));
			this._BinaryTrees = new EntitySet<BinaryTree>(new Action<BinaryTree>(this.attach_BinaryTrees), new Action<BinaryTree>(this.detach_BinaryTrees));
			this._BinaryTree1 = default(EntityRef<BinaryTree>);
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this.OnIdChanging(value);
					this.SendPropertyChanging();
					this._Id = value;
					this.SendPropertyChanged("Id");
					this.OnIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ParentId
		{
			get
			{
				return this._ParentId;
			}
			set
			{
				if ((this._ParentId != value))
				{
					if (this._BinaryTree1.HasLoadedOrAssignedValue)
					{
						throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException();
					}
					this.OnParentIdChanging(value);
					this.SendPropertyChanging();
					this._ParentId = value;
					this.SendPropertyChanged("ParentId");
					this.OnParentIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this.OnNameChanging(value);
					this.SendPropertyChanging();
					this._Name = value;
					this.SendPropertyChanged("Name");
					this.OnNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(300) NOT NULL", CanBeNull=false)]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this.OnDescriptionChanging(value);
					this.SendPropertyChanging();
					this._Description = value;
					this.SendPropertyChanged("Description");
					this.OnDescriptionChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ext", DbType="Char(10) NOT NULL", CanBeNull=false)]
		public string ext
		{
			get
			{
				return this._ext;
			}
			set
			{
				if ((this._ext != value))
				{
					this.OnextChanging(value);
					this.SendPropertyChanging();
					this._ext = value;
					this.SendPropertyChanged("ext");
					this.OnextChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="BinaryTree_BinaryTable", Storage="_BinaryTables", ThisKey="Id", OtherKey="ParentId")]
		public EntitySet<BinaryTable> BinaryTables
		{
			get
			{
				return this._BinaryTables;
			}
			set
			{
				this._BinaryTables.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="BinaryTree_BinaryTree", Storage="_BinaryTrees", ThisKey="Id", OtherKey="ParentId")]
		public EntitySet<BinaryTree> BinaryTrees
		{
			get
			{
				return this._BinaryTrees;
			}
			set
			{
				this._BinaryTrees.Assign(value);
			}
		}
		
		[global::System.Data.Linq.Mapping.AssociationAttribute(Name="BinaryTree_BinaryTree", Storage="_BinaryTree1", ThisKey="ParentId", OtherKey="Id", IsForeignKey=true)]
		public BinaryTree BinaryTree1
		{
			get
			{
				return this._BinaryTree1.Entity;
			}
			set
			{
				BinaryTree previousValue = this._BinaryTree1.Entity;
				if (((previousValue != value) 
							|| (this._BinaryTree1.HasLoadedOrAssignedValue == false)))
				{
					this.SendPropertyChanging();
					if ((previousValue != null))
					{
						this._BinaryTree1.Entity = null;
						previousValue.BinaryTrees.Remove(this);
					}
					this._BinaryTree1.Entity = value;
					if ((value != null))
					{
						value.BinaryTrees.Add(this);
						this._ParentId = value.Id;
					}
					else
					{
						this._ParentId = default(System.Guid);
					}
					this.SendPropertyChanged("BinaryTree1");
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		
		private void attach_BinaryTables(BinaryTable entity)
		{
			this.SendPropertyChanging();
			entity.BinaryTree = this;
		}
		
		private void detach_BinaryTables(BinaryTable entity)
		{
			this.SendPropertyChanging();
			entity.BinaryTree = null;
		}
		
		private void attach_BinaryTrees(BinaryTree entity)
		{
			this.SendPropertyChanging();
			entity.BinaryTree1 = this;
		}
		
		private void detach_BinaryTrees(BinaryTree entity)
		{
			this.SendPropertyChanging();
			entity.BinaryTree1 = null;
		}
	}
	
	public partial class SelectBinaryTreeResult
	{
		
		private System.Guid _Id;
		
		private System.Guid _ParentId;
		
		private string _Name;
		
		private string _Description;
		
		private string _ext;
		
		public SelectBinaryTreeResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this._Id = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ParentId
		{
			get
			{
				return this._ParentId;
			}
			set
			{
				if ((this._ParentId != value))
				{
					this._ParentId = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this._Name = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(300) NOT NULL", CanBeNull=false)]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this._Description = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ext", DbType="Char(10) NOT NULL", CanBeNull=false)]
		public string ext
		{
			get
			{
				return this._ext;
			}
			set
			{
				if ((this._ext != value))
				{
					this._ext = value;
				}
			}
		}
	}
	
	public partial class InsertBinaryResult
	{
		
		private System.Guid _Id;
		
		public InsertBinaryResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this._Id = value;
				}
			}
		}
	}
	
	public partial class InsertBinaryNodeResult
	{
		
		private System.Guid _Id;
		
		public InsertBinaryNodeResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this._Id = value;
				}
			}
		}
	}
	
	public partial class SelectBinaryResult
	{
		
		private System.Data.Linq.Binary _Data;
		
		private string _Ext;
		
		public SelectBinaryResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Data", DbType="Image NOT NULL", CanBeNull=false)]
		public System.Data.Linq.Binary Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				if ((this._Data != value))
				{
					this._Data = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Ext", DbType="VarChar(10) NOT NULL", CanBeNull=false)]
		public string Ext
		{
			get
			{
				return this._Ext;
			}
			set
			{
				if ((this._Ext != value))
				{
					this._Ext = value;
				}
			}
		}
	}
	
	public partial class SelectBinaryContentsResult
	{
		
		private System.Guid _Id;
		
		private string _Name;
		
		private string _Description;
		
		private string _Length;
		
		public SelectBinaryContentsResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this._Id = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this._Name = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(300) NOT NULL", CanBeNull=false)]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this._Description = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Length", DbType="NVarChar(30) NOT NULL", CanBeNull=false)]
		public string Length
		{
			get
			{
				return this._Length;
			}
			set
			{
				if ((this._Length != value))
				{
					this._Length = value;
				}
			}
		}
	}
	
	public partial class SelectBinaryTableResult
	{
		
		private System.Guid _Id;
		
		private System.Guid _ParentId;
		
		private string _Name;
		
		private string _Description;
		
		private string _Ext;
		
		public SelectBinaryTableResult()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Id", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				if ((this._Id != value))
				{
					this._Id = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ParentId", DbType="UniqueIdentifier NOT NULL")]
		public System.Guid ParentId
		{
			get
			{
				return this._ParentId;
			}
			set
			{
				if ((this._ParentId != value))
				{
					this._ParentId = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Name", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				if ((this._Name != value))
				{
					this._Name = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Description", DbType="NVarChar(300) NOT NULL", CanBeNull=false)]
		public string Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				if ((this._Description != value))
				{
					this._Description = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Ext", DbType="VarChar(10) NOT NULL", CanBeNull=false)]
		public string Ext
		{
			get
			{
				return this._Ext;
			}
			set
			{
				if ((this._Ext != value))
				{
					this._Ext = value;
				}
			}
		}
	}
}
#pragma warning restore 1591
