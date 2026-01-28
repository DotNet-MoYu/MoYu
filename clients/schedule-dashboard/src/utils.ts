import dayjs from "dayjs";
import "dayjs/locale/zh-cn";
import relativeTime from "dayjs/plugin/relativeTime";
import utc from "dayjs/plugin/utc";
import apiconfig from "./apiconfig";

dayjs.extend(relativeTime);
dayjs.extend(utc);
dayjs.locale("zh-cn");

export function dayTimeUtc(date: dayjs.ConfigType) {
  return dayjs.utc(date);
}

export function dayTime(date: dayjs.ConfigType) {
  if (apiconfig.useUtcTimestamp === "true") {
    return dayTimeUtc(date);
  } else {
    return dayjs(date);
  }
}

export function dayFromNow(date: dayjs.ConfigType) {
  return dayTime(date).fromNow();
}

export function findMaxUtcTimeString(utcTimeStrings: string[]) {
  if (utcTimeStrings.length === 0) {
    throw new Error("数组不能为空");
  }

  let maxDate = null;
  let maxTimeString = "";

  for (let i = 0; i < utcTimeStrings.length; i++) {
    const timeString = utcTimeStrings[i];
    const date = new Date(timeString);

    if (isNaN(date.getTime())) {
      throw new Error(`无效的UTC时间字符串: ${timeString}`);
    }

    if (maxDate === null || date > maxDate) {
      maxDate = date;
      maxTimeString = timeString;
    }
  }

  return maxTimeString;
}

export function findMinUtcTimeString(utcTimeStrings: string[]) {
  if (utcTimeStrings.length === 0) {
    throw new Error("数组不能为空");
  }

  let minDate = new Date(utcTimeStrings[0]);
  let minTimeString = utcTimeStrings[0];

  if (isNaN(minDate.getTime())) {
    throw new Error(`无效的UTC时间字符串: ${utcTimeStrings[0]}`);
  }

  for (let i = 1; i < utcTimeStrings.length; i++) {
    const timeString = utcTimeStrings[i];
    const date = new Date(timeString);

    if (isNaN(date.getTime())) {
      throw new Error(`无效的UTC时间字符串: ${timeString}`);
    }

    if (date < minDate) {
      minDate = date;
      minTimeString = timeString;
    }
  }

  return minTimeString;
}

export function formatDuration(ms: number): string {
  if (ms < 1000) {
    return `${ms}ms`;
  }

  const seconds = ms / 1000;
  if (seconds < 60) {
    const val = Math.round(seconds * 10) / 10;
    if (val >= 60) return formatDuration(val * 1000);
    return val % 1 === 0 ? `${val}s` : `${val.toFixed(1)}s`;
  }

  const minutes = seconds / 60;
  if (minutes < 60) {
    const val = Math.round(minutes * 10) / 10;
    if (val >= 60) return formatDuration(val * 60 * 1000);
    return val % 1 === 0 ? `${val}min` : `${val.toFixed(1)}min`;
  }

  const hours = minutes / 60;
  if (hours < 24) {
    const val = Math.round(hours * 10) / 10;
    if (val >= 24) return formatDuration(val * 60 * 60 * 1000);
    return val % 1 === 0 ? `${val}h` : `${val.toFixed(1)}h`;
  }

  const days = hours / 24;
  if (days < 365) {
    const val = Math.round(days * 10) / 10;
    if (val >= 365) return formatDuration(val * 24 * 60 * 60 * 1000);
    return val % 1 === 0 ? `${val}d` : `${val.toFixed(1)}d`;
  }

  const years = days / 365;
  const val = Math.round(years * 10) / 10;
  return val % 1 === 0 ? `${val}y` : `${val.toFixed(1)}y`;
}
