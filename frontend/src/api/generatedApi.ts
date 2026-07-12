import { emptySplitApi as api } from "./emptyApi";
const injectedRtkApi = api.injectEndpoints({
  endpoints: (build) => ({
    getStatus: build.query<GetStatusApiResponse, GetStatusApiArg>({
      query: () => ({ url: `/api/status` }),
    }),
    getHabits: build.query<GetHabitsApiResponse, GetHabitsApiArg>({
      query: (queryArg) => ({
        url: `/api/habits`,
        params: {
          includeArchived: queryArg.includeArchived,
        },
      }),
    }),
    createHabit: build.mutation<CreateHabitApiResponse, CreateHabitApiArg>({
      query: (queryArg) => ({
        url: `/api/habits`,
        method: "POST",
        body: queryArg.habitInput,
      }),
    }),
    getHabit: build.query<GetHabitApiResponse, GetHabitApiArg>({
      query: (queryArg) => ({ url: `/api/habits/${queryArg.id}` }),
    }),
    updateHabit: build.mutation<UpdateHabitApiResponse, UpdateHabitApiArg>({
      query: (queryArg) => ({
        url: `/api/habits/${queryArg.id}`,
        method: "PUT",
        body: queryArg.habitInput,
      }),
    }),
    archiveHabit: build.mutation<ArchiveHabitApiResponse, ArchiveHabitApiArg>({
      query: (queryArg) => ({
        url: `/api/habits/${queryArg.id}`,
        method: "DELETE",
      }),
    }),
    reorderHabits: build.mutation<
      ReorderHabitsApiResponse,
      ReorderHabitsApiArg
    >({
      query: (queryArg) => ({
        url: `/api/habits/reorder`,
        method: "PUT",
        body: queryArg.body,
      }),
    }),
    getHabitHistory: build.query<
      GetHabitHistoryApiResponse,
      GetHabitHistoryApiArg
    >({
      query: (queryArg) => ({
        url: `/api/habits/${queryArg.id}/history`,
        params: {
          historyDays: queryArg.historyDays,
          today: queryArg.today,
        },
      }),
    }),
    getDay: build.query<GetDayApiResponse, GetDayApiArg>({
      query: (queryArg) => ({ url: `/api/days/${queryArg.date}` }),
    }),
    upsertEntry: build.mutation<UpsertEntryApiResponse, UpsertEntryApiArg>({
      query: (queryArg) => ({
        url: `/api/entries`,
        method: "PUT",
        body: queryArg.entryUpsertRequest,
      }),
    }),
  }),
  overrideExisting: false,
});
export { injectedRtkApi as enhancedApi };
export type GetStatusApiResponse = /** status 200 OK */ StatusResponse;
export type GetStatusApiArg = void;
export type GetHabitsApiResponse = /** status 200 OK */ HabitView[];
export type GetHabitsApiArg = {
  includeArchived?: boolean;
};
export type CreateHabitApiResponse = /** status 201 Created */ HabitView;
export type CreateHabitApiArg = {
  habitInput: HabitInput;
};
export type GetHabitApiResponse = /** status 200 OK */ HabitView;
export type GetHabitApiArg = {
  id: number;
};
export type UpdateHabitApiResponse = /** status 200 OK */ HabitView;
export type UpdateHabitApiArg = {
  id: number;
  habitInput: HabitInput;
};
export type ArchiveHabitApiResponse = unknown;
export type ArchiveHabitApiArg = {
  id: number;
};
export type ReorderHabitsApiResponse = unknown;
export type ReorderHabitsApiArg = {
  body: (number | string)[];
};
export type GetHabitHistoryApiResponse = /** status 200 OK */ HabitHistory;
export type GetHabitHistoryApiArg = {
  id: number;
  historyDays?: number | string;
  today?: string;
};
export type GetDayApiResponse = /** status 200 OK */ DayView;
export type GetDayApiArg = {
  date: string;
};
export type UpsertEntryApiResponse = /** status 200 OK */ HabitDayView;
export type UpsertEntryApiArg = {
  entryUpsertRequest: EntryUpsertRequest;
};
export type StatusResponse = {
  version: string;
  friendlyStatus: string;
  timestamp: string;
};
export type HabitType = number;
export type HabitView = {
  id: number | string;
  name: string;
  type: HabitType;
  unit: null | string;
  target: null | number | string;
  sortOrder: number | string;
  isArchived: boolean;
};
export type HabitInput = {
  name: string;
  type: HabitType;
  unit?: null | string;
  target?: null | number | string;
};
export type ProblemDetails = {
  type?: null | string;
  title?: null | string;
  status?: null | number | string;
  detail?: null | string;
  instance?: null | string;
};
export type DayCompletion = {
  date: string;
  isComplete: boolean;
  value: null | number | string;
};
export type HabitHistory = {
  habitId: number | string;
  name: string;
  currentStreak: number | string;
  longestStreak: number | string;
  days: DayCompletion[];
};
export type HabitDayView = {
  habitId: number | string;
  name: string;
  type: HabitType;
  unit: null | string;
  target: null | number | string;
  sortOrder: number | string;
  value: null | number | string;
  isComplete: boolean;
  currentStreak: number | string;
};
export type DayView = {
  date: string;
  habits: HabitDayView[];
};
export type EntryUpsertRequest = {
  habitId: number | string;
  date: string;
  value: number | string;
};
export const {
  useGetStatusQuery,
  useGetHabitsQuery,
  useCreateHabitMutation,
  useGetHabitQuery,
  useUpdateHabitMutation,
  useArchiveHabitMutation,
  useReorderHabitsMutation,
  useGetHabitHistoryQuery,
  useGetDayQuery,
  useUpsertEntryMutation,
} = injectedRtkApi;
