type CashFlowChartBar = {
  x: number;
  y: number;
  width: number;
  height: number;
  value: number;
  label: string;
  isFinal: boolean;
};

type CashFlowChartVm = {
  width: number;
  height: number;
  bars: CashFlowChartBar[];
  maxValue: number;
};