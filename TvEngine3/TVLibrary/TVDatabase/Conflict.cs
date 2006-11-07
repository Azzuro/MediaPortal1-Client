//========================================================================
// This file was generated using the MyGeneration tool in combination
// with the Gentle.NET Business Entity template, $Rev: 942 $
//========================================================================
using System;
using System.Collections;
using Gentle.Common;
using Gentle.Framework;

namespace TvDatabase
{
	/// <summary>
	/// Instances of this class represent the properties and methods of a row in the table <b>Conflicts</b>.
	/// </summary>
	[TableName("Conflict", CacheStrategy.Temporary)]
	public class Conflict : Persistent
	{
		#region Members
		private bool isChanged;
		[TableColumn("idConflict", NotNull=true), PrimaryKey(AutoGenerated=true)]
		protected int idConflict;
		[TableColumn("idSchedule", NotNull=true)]
		protected int idSchedule;
		[TableColumn("idConflictingSchedule", NotNull=true)]
		protected int idConflictingSchedule;
		[TableColumn("idChannel", NotNull=true)]
		protected int idChannel;
		[TableColumn("conflictDate", NotNull=true)]
		protected DateTime conflictDate;
		[TableColumn("idCard", NullValue=0)]
		protected int idCard;
		#endregion
			
		#region Constructors
		/// <summary> 
		/// Create a new object using the minimum required information (all not-null fields except 
		/// auto-generated primary keys). 
		/// </summary> 
		public Conflict(int idSchedule, int idConflictingSchedule, int idChannel, DateTime conflictDate)
		{
			isChanged = true;
			this.idSchedule = idSchedule;
			this.idConflictingSchedule = idConflictingSchedule;
			this.idChannel = idChannel;
			this.conflictDate = conflictDate;
      this.idCard = 0;
		}

		/// <summary> 
		/// Create a new object by specifying all fields (except the auto-generated primary key field). 
		/// </summary> 
    public Conflict(int idSchedule, int idConflictingSchedule, int idChannel, DateTime conflictDate, int idCard)
		{
			isChanged = true;
			this.idSchedule = idSchedule;
			this.idConflictingSchedule = idConflictingSchedule;
			this.idChannel = idChannel;
			this.conflictDate = conflictDate;
			this.idCard = idCard;
		}
			
		/// <summary> 
		/// Create an object from an existing row of data. This will be used by Gentle to 
		/// construct objects from retrieved rows. 
		/// </summary> 
    public Conflict(int idConflict, int idSchedule, int idConflictingSchedule, int idChannel, DateTime conflictDate, int idCard)
		{
			this.idConflict = idConflict;
			this.idSchedule = idSchedule;
			this.idConflictingSchedule = idConflictingSchedule;
			this.idChannel = idChannel;
			this.conflictDate = conflictDate;
			this.idCard = idCard;
		}
		#endregion

		#region Public Properties
		public bool IsChanged
		{
			get	{ return isChanged; }
		}

		/// <summary>
		/// Property relating to database column idConflict
		/// </summary>
    public int IdConflict
		{
			get { return idConflict; }
		}

		/// <summary>
		/// Property relating to database column idSchedule
		/// </summary>
    public int IdSchedule
		{
			get { return idSchedule; }
			set { isChanged |= idSchedule != value; idSchedule = value; }
		}

		/// <summary>
		/// Property relating to database column idConflictingSchedule
		/// </summary>
    public int IdConflictingSchedule
		{
			get { return idConflictingSchedule; }
			set { isChanged |= idConflictingSchedule != value; idConflictingSchedule = value; }
		}

		/// <summary>
		/// Property relating to database column idChannel
		/// </summary>
    public int IdChannel
		{
			get { return idChannel; }
			set { isChanged |= idChannel != value; idChannel = value; }
		}

		/// <summary>
		/// Property relating to database column conflictDate
		/// </summary>
    public DateTime ConflictDate
		{
			get { return conflictDate; }
			set { isChanged |= conflictDate != value; conflictDate = value; }
		}

		/// <summary>
		/// Property relating to database column idCard
		/// </summary>
    public int IdCard
		{
			get { return idCard; }
			set { isChanged |= idCard != value; idCard = value; }
		}
		#endregion

		#region Storage and Retrieval
	
		/// <summary>
		/// Static method to retrieve all instances that are stored in the database in one call
		/// </summary>
		static public IList ListAll()
		{
			return Broker.RetrieveList(typeof(Conflict));
		}

    public static Conflict Retrieve(int id)
		{
			Key key = new Key(typeof(Conflict), true, "idConflict", id);
			return Broker.RetrieveInstance(typeof(Conflict), key) as Conflict;
		}

    public static Conflict ComplexRetrieve(int id)
		{
			throw new NotImplementedException("Gentle.NET Business Entity script: Generation of complex retrieve function (multiple primary keys) is not implemented yet.");
			return null;
		}
		
		public override void Persist()
		{
			if (IsChanged || !IsPersisted)
			{
				base.Persist();
				isChanged = false;
			}
		}

		#endregion

	}
}
