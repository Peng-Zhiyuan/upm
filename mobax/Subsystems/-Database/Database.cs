using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Database : StuffObject<Database>
{
    public static Func<long> RequestTimestap;

    public RoleDatabase roleDatabase = new RoleDatabase();
    public ItemDatabase itemDatabase = new ItemDatabase();
    public DrawCardDatabase drawCardDatabase = new DrawCardDatabase(); // 抽卡
    public FormationDatabase FormationDatabase = new FormationDatabase(); // 编队
    public MailDatabase mailDatabase = new MailDatabase();
    public GameTaskDatabase taskDatabase = new GameTaskDatabase();
    public DailyDatabase dailyDatabase = new DailyDatabase();
    public StageDatabase stageDatabase = new StageDatabase();
    public RecordDatabase recordDatabase = new RecordDatabase();
    public ActiveDatabase activeDatabase = new ActiveDatabase();
    public OrderDatabase orderDatabase = new OrderDatabase();
    public GiftDatabase giftDatabase = new GiftDatabase();

    public async Task FetchAllAsync()
    {
        var taskList = new List<Task>();
        taskList.Add(this.roleDatabase.FetchMyselfAsync());
        taskList.Add(this.itemDatabase.ResetByFetchAllAsync());
        taskList.Add(this.drawCardDatabase.FetchDrawCardAsync());
        taskList.Add(this.FormationDatabase.FetchFormationAsync());
        taskList.Add(this.mailDatabase.UpdateByFetchNewModifiedSinceLastUpdateAsync());
        taskList.Add(this.taskDatabase.ResetByFetchAllAsync());
        taskList.Add(this.dailyDatabase.ResetByFetchTodayAsync());
        taskList.Add(StageManager.Stuff.FetchStagesAsync());
        taskList.Add(this.stageDatabase.FetchStageChapterAsync());
        taskList.Add(this.recordDatabase.ResetByFetchAllAsync());
        taskList.Add(this.activeDatabase.SyncAsync());
        taskList.Add(this.orderDatabase.ResetByFetchAllAsync());
        taskList.Add(this.giftDatabase.ResetByFetchAllAsync());
        await Task.WhenAll(taskList);
    }

    public List<DatabaseTransaction> PopRecordedTransaction()
    {
        var ret = new List<DatabaseTransaction>(this.transactionRecordList);
        this.transactionRecordList.Clear();
        return ret;
    }

    List<DatabaseTransaction> transactionRecordList = new List<DatabaseTransaction>();

    // [{"_id":"102x2xekp","id":15001,"t":5,"k":"","v":1,"b":15,"r":{"_id":"102x2xekp","id":15001,"uid":"102x2","val":1,"bag":15,"attach":[1,0,0,1,1]}}]
    public void ApplyTransaction(DatabaseTransaction transaction, long timestampMs, bool recardTransaction)
    {
        var t = transaction.t;
        if (t == TransactionType.NoUse)
        {
            return;
        }
        var rowId = transaction.id;

        // 临时处理为0的时候更新role数据
        string dataModel;
        if (rowId == 0)
        {
            dataModel = ServerDataModel.Role;
        }
        else
        {
            dataModel = StaticDataUtil.GetServerDataModel(rowId);
        }
        Debug.Log($"[Database] Apply Transaction: [rowId：{rowId}, dataModel: {dataModel}, operation: {transaction.t}]");
        switch (dataModel)
        {
            case ServerDataModel.Item:
                this.itemDatabase.ApplyTransaction(transaction);
                break;
            case ServerDataModel.Role:
                this.roleDatabase.ApplyTransaction(transaction);
                //FormationUtil.UpdateDataFromDatabase();
                break;
            case ServerDataModel.Record:
                this.recordDatabase.ApplyTransaction(transaction);
                break;
            case ServerDataModel.Daily:
                this.dailyDatabase.ApplyTransaction(transaction, timestampMs);
                break;
            default:
                throw new Exception("[Database] dataModel `" + dataModel + "` not supported");
        }
        if (recardTransaction)
        {
            transactionRecordList.Add(transaction);
        }

        // 额外处理
        if (dataModel == ServerDataModel.Item)
        {
            var itype = StaticDataUtil.GetIType(rowId);
            if (itype == IType.Hero)
            {
                HeroManager.Instance.UploadHeroPowerIfNeed(rowId);
            }
        }
    }

    public int ItemRowKeyToGetCount(int rowID)
    {
        string dataModel = StaticDataUtil.GetServerDataModel(rowID);
        int value = 0;
        if (dataModel == ServerDataModel.Item)
        {
            value = itemDatabase.GetHoldCount(rowID);
        }
        else if (dataModel == ServerDataModel.Role)
        {
            ItemRow itemRow = StaticData.ItemTable.TryGet(rowID);
            if (itemRow != null
                && !string.IsNullOrEmpty(itemRow.Key))
            {
                value = ReflectionUtil.GetField<int>(roleDatabase.Me, itemRow.Key);
            }
        }
        return value;
    }
}