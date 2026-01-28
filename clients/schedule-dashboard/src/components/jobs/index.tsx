import {
  IconDelete,
  IconMore,
  IconPlayCircle,
  IconSearch,
  IconStop,
  IconUploadError,
  IconVigoLogo,
} from "@douyinfe/semi-icons";
import {
  Descriptions,
  Divider,
  Dropdown,
  Input,
  Popconfirm,
  Popover,
  Space,
  Table,
  Tag,
  TextArea,
  Toast,
  Tooltip,
  Typography,
} from "@douyinfe/semi-ui";
import { Data } from "@douyinfe/semi-ui/lib/es/descriptions";
import {
  ExpandedRowRender,
  OnRow,
} from "@douyinfe/semi-ui/lib/es/table/interface";
import {
  useCallback,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
} from "react";
import useFetch from "use-http";
import { JobDetail, Scheduler, TriggerTimeline } from "../../types";
import apiconfig from "../../apiconfig";
import columns from "./columns";
import RenderValue from "./render-value";
import FlipClockCountdown from "@leenguyen/react-flip-clock-countdown";
import { dayFromNow, dayTime, formatDuration } from "../../utils";
import styles from "./index.module.css";
import clsx from "clsx";

const style = {
  boxShadow: "var(--semi-shadow-elevated)",
  backgroundColor: "var(--semi-color-bg-2)",
  borderRadius: "4px",
  padding: "10px",
  margin: "10px",
  width: "350px",
};

function getOValueByData(key: string, expandData: Data[]) {
  var item = expandData.find((u) => u.key === key) as any;
  return item?.ovalue || null;
}

export default function Jobs({ mode }: { mode: string }) {
  /**
   * 作业状态
   */
  const [jobs, setJobs] = useState<Scheduler[]>([]);
  const [words, setWords] = useState<string>();
  const deferredWords = useDeferredValue(words);
  const [allTimelines, setAllTimelines] = useState<TriggerTimeline[]>([]);

  const jobList = useMemo(() => {
    if (!deferredWords || deferredWords.trim().length === 0) {
      return jobs;
    }

    const trimWords = deferredWords.trim();

    return jobs.filter(
      (u) =>
        (u.jobDetail?.jobId ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.groupName ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.description ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.jobType ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.assemblyName ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.properties ?? "").indexOf(trimWords) > -1 ||
        (u.jobDetail?.concurrent === true ? "并行" : "串行").indexOf(
          trimWords
        ) > -1 ||
        (u.jobDetail?.temporary === true ? "临时" : "").indexOf(trimWords) >
          -1 ||
        // ==== 触发器搜索
        (u.triggers || []).findIndex(
          (t) =>
            (t.triggerId ?? "").indexOf(trimWords) > -1 ||
            (t.description ?? "").indexOf(trimWords) > -1 ||
            (t.triggerType ?? "").indexOf(trimWords) > -1 ||
            (t.assemblyName ?? "").indexOf(trimWords) > -1 ||
            (t.args ?? "").indexOf(trimWords) > -1
        ) > -1
    );
  }, [jobs, deferredWords]);

  /**
   * 初始化请求配置
   */
  const { post, response } = useFetch(apiconfig.hostAddress, apiconfig.options);

  /**
   * 获取内存中所有作业
   */
  const loadJobs = async () => {
    const data = await post("/get-jobs");
    if (response.ok) setJobs((s) => data);
  };

  /**
   * 获取内存中所有运行记录
   */
  const loadAllTimelines = async () => {
    const data = await post("/timelines-log");
    if (response.ok) setAllTimelines((s) => data);
  };

  /**
   * 操作作业触发器
   */
  const callAction = async (
    jobid: string,
    triggerid: string,
    action: string
  ) => {
    await post(
      "/operate-trigger?jobid=" +
        jobid +
        "&triggerid=" +
        triggerid +
        "&action=" +
        action
    );

    if (response.ok) {
      Toast.success({
        content: "操作成功",
        duration: 3,
      });
    } else {
      Toast.error({
        content: "操作失败",
        duration: 3,
      });
    }
  };

  /**
   * 生成表格类型数据
   */
  const data: JobDetail[] = useMemo(() => {
    const jobDetails: JobDetail[] = [];
    if (!jobList || jobList.length === 0) return jobDetails;

    for (const scheduler of jobList) {
      let jobDetail = scheduler.jobDetail!;
      jobDetail.triggers = scheduler.triggers;
      jobDetail.refreshDate = new Date();

      if (
        apiconfig.displayEmptyTriggerJobs === "false" &&
        scheduler.triggers?.length === 0
      ) {
        continue;
      }

      jobDetails.push(jobDetail);
    }

    return jobDetails;
  }, [jobList]);

  useEffect(() => {
    loadJobs();
    loadAllTimelines();

    var eventSource = new EventSource(apiconfig.hostAddress + "/check-change");

    eventSource.onmessage = function (e) {
      loadJobs();
      loadAllTimelines();
    };

    return () => {
      eventSource.close();
    };
  }, []);

  /**
   * 展开行渲染
   */
  const expandRowRender: ExpandedRowRender<JobDetail> = useCallback(
    (jobDetail, index) => {
      // 查找作业计划
      var scheduler = jobList.find(
        (u) => u.jobDetail?.jobId === jobDetail?.jobId
      );
      if (!scheduler) return <></>;

      // 构建触发器列表
      const triggerData: Data[][] = [];
      for (const trigger of scheduler?.triggers!) {
        const triggerItem: Data[] = [];
        for (const prop in trigger) {
          triggerItem.push({
            key: prop.charAt(0).toUpperCase() + prop.slice(1),
            value: (
              <RenderValue
                prop={prop}
                value={trigger[prop]}
                trigger={trigger}
              />
            ),
            ovalue: trigger[prop],
          } as Data);
        }

        triggerData.push(triggerItem);
      }

      return (
        <div style={{ display: "flex", flexWrap: "wrap" }}>
          {triggerData.map((expandData, index) => (
            <div
              style={style}
              key={
                getOValueByData("TriggerId", expandData).toString() +
                "_" +
                getOValueByData("JobId", expandData).toString() +
                index
              }
            >
              <div
                style={{
                  marginTop: 3,
                  marginRight: 5,
                  marginLeft: 5,
                  display: "flex",
                  justifyContent: "space-between",
                }}
              >
                {Number(getOValueByData("Status", expandData)) === 3 ? (
                  <Tooltip content="启动">
                    <IconPlayCircle
                      style={{ color: "red", cursor: "pointer" }}
                      size="large"
                      onClick={() =>
                        callAction(
                          getOValueByData("JobId", expandData).toString(),
                          getOValueByData("TriggerId", expandData).toString(),
                          "start"
                        )
                      }
                    />
                  </Tooltip>
                ) : (
                  <span></span>
                )}
                <FlipClockCountdown
                  to={getOValueByData("NextRunTime", expandData)}
                  labels={["天", "时", "分", "秒"]}
                  labelStyle={{
                    fontSize: 12,
                    fontWeight: 500,
                    color: "var(--semi-color-text-0)",
                  }}
                  digitBlockStyle={{ width: 20, height: 30, fontSize: 15 }}
                  hideOnComplete={false}
                />
                <Dropdown
                  render={
                    <Dropdown.Menu>
                      <Dropdown.Item
                        onClick={() =>
                          callAction(
                            getOValueByData("JobId", expandData).toString(),
                            getOValueByData("TriggerId", expandData).toString(),
                            "start"
                          )
                        }
                      >
                        <IconPlayCircle size="extra-large" /> 启动
                      </Dropdown.Item>
                      <Dropdown.Item
                        onClick={() =>
                          callAction(
                            getOValueByData("JobId", expandData).toString(),
                            getOValueByData("TriggerId", expandData).toString(),
                            "pause"
                          )
                        }
                      >
                        <IconStop size="extra-large" /> 暂停
                      </Dropdown.Item>
                      <Dropdown.Item>
                        <Popconfirm
                          zIndex={10000000}
                          title={
                            "确定要删除当前触发器 [" +
                            getOValueByData(
                              "TriggerId",
                              expandData
                            ).toString() +
                            "]？"
                          }
                          onConfirm={() =>
                            callAction(
                              getOValueByData("JobId", expandData).toString(),
                              getOValueByData(
                                "TriggerId",
                                expandData
                              ).toString(),
                              "remove"
                            )
                          }
                        >
                          <IconDelete size="small" /> &nbsp;删除
                        </Popconfirm>
                      </Dropdown.Item>
                      <Dropdown.Item
                        onClick={() =>
                          callAction(
                            getOValueByData("JobId", expandData).toString(),
                            getOValueByData("TriggerId", expandData).toString(),
                            "run"
                          )
                        }
                      >
                        <IconVigoLogo size="extra-large" /> 手动执行
                      </Dropdown.Item>
                    </Dropdown.Menu>
                  }
                >
                  <IconMore style={{ cursor: "pointer" }} size="large" />
                </Dropdown>
              </div>

              <Divider margin="8px" />
              <Descriptions align="left" data={expandData} />
            </div>
          ))}
        </div>
      );
    },
    [jobList]
  );

  const handleRow: OnRow<JobDetail> = (jobDetail, index) => {
    // 给偶数行设置斑马纹
    if (index! % 2 === 0) {
      return {
        style: {
          background: "var(--semi-color-fill-0)",
        },
      };
    } else {
      return {};
    }
  };

  var invalidJobCount = data.filter(
    (u) => (u.triggers?.length || 0) === 0
  ).length;

  return (
    <>
      <div
        style={{
          border: "1px solid var(--semi-color-border)",
          borderRadius: "10px",
        }}
      >
        <Input
          prefix={<IconSearch />}
          showClear
          placeholder="搜索关键字..."
          value={words}
          onChange={(val) => setWords(val)}
          autoFocus
        />
        <Table
          rowKey="jobId"
          columns={columns}
          dataSource={data}
          onRow={handleRow}
          expandedRowRender={expandRowRender}
          pagination={false}
          resizable
          bordered
          expandRowByClick
          expandAllRows={apiconfig.defaultExpandAllJobs === "true"}
          rowExpandable={(jobDetail) =>
            !!(
              jobDetail?.jobId &&
              jobList.find((u) => u.jobDetail?.jobId === jobDetail?.jobId)
                ?.triggers?.length !== 0
            )
          }
        />
        <Typography.Paragraph type="secondary" style={{ padding: 10 }}>
          {(words?.trim().length || 0) > 0 ? (
            <>
              搜索 "<b>{words?.trim()}</b>" 共 <b>{jobList.length}</b> 项结果。
            </>
          ) : (
            <>
              共有 <b>{data.length}</b> 项作业任务
              {invalidJobCount > 0 ? (
                <>
                  ， 其中 <b>{invalidJobCount}</b> 项未设置触发器
                </>
              ) : (
                <></>
              )}
              。
            </>
          )}
        </Typography.Paragraph>
      </div>
      {(words?.trim().length || 0) === 0 && (
        <div>
          <Typography.Title heading={5} style={{ margin: "24px 0 16px 0" }}>
            # 运行记录
          </Typography.Title>
          <div style={{ color: "var(--semi-color-text-0)" }}>
            {allTimelines.map((timeline, i) => (
              <div
                key={timeline.jobId! + timeline.triggerId! + i}
                style={{ marginBottom: 8, fontSize: 14 }}
                className={clsx(
                  styles.timelineItem,
                  mode === "dark" && styles.dark
                )}
              >
                <Tag size="large" color="green" type="light">
                  {timeline.jobId}
                </Tag>{" "}
                <Tag size="large" color="green" type="light">
                  {timeline.triggerId}
                </Tag>{" "}
                <Tag color="grey" type="light">
                  {dayTime(timeline.lastRunTime).format("YYYY/MM/DD HH:mm:ss")}(
                  {dayFromNow(timeline.lastRunTime)})
                </Tag>{" "}
                第{" "}
                <Tag color="green" type="light">
                  {timeline.numberOfRuns}
                </Tag>{" "}
                次运行，耗时{" "}
                <Tooltip
                  content={<>{timeline.elapsedTime}ms</>}
                  zIndex={10000000001}
                >
                  <Tag color="lime" type="light">
                    {formatDuration(timeline.elapsedTime!)}
                  </Tag>
                </Tooltip>{" "}
                {timeline.mode === 1 && (
                  <Tag color="yellow" type="solid">
                    手动
                  </Tag>
                )}
                {timeline.exception && (
                  <Popover
                    showArrow
                    content={
                      <div
                        className="exception-box"
                        style={{
                          padding: 10,
                          width: 400,
                        }}
                      >
                        <TextArea value={timeline.exception} rows={10} />
                      </div>
                    }
                    trigger="click"
                    zIndex={10000000002}
                  >
                    <IconUploadError
                      style={{
                        position: "relative",
                        color: "red",
                        top: 4,
                        cursor: "pointer",
                        marginLeft: 5,
                      }}
                    />
                  </Popover>
                )}
                {timeline.result && (
                  <div>
                    <Typography.Paragraph
                      ellipsis={{
                        rows: 2,
                        expandable: true,
                        expandText: "展开",
                        collapsible: true,
                        collapseText: "折叠",
                      }}
                      style={{ width: 200 }}
                      copyable
                    >
                      {timeline.result}
                    </Typography.Paragraph>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </>
  );
}
