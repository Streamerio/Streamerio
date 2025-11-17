import { AsyncPromptConfig, Separator } from '@inquirer/core';
type Choice<Value> = {
    value: Value;
    name?: string;
    description?: string;
    disabled?: boolean | string;
    type?: never;
};
export type ChoiceOrSeparatorArray<Value> = ReadonlyArray<Choice<Value> | Separator>;
declare const _default: <Value extends unknown>(config: AsyncPromptConfig & {
    source: (input?: string | undefined) => Promise<ChoiceOrSeparatorArray<Value>>;
    validate?: ((value: Value) => string | boolean | Promise<string | boolean>) | undefined;
    transformer?: ((value: string, { isFinal }: {
        isFinal: boolean;
    }) => string) | undefined;
    default?: string | undefined;
    emptyText?: string | undefined;
    pageSize?: number | undefined;
    searchText?: string | undefined;
    suggestOnly?: boolean | undefined;
}, context?: import("@inquirer/type").Context | undefined) => import("@inquirer/type").CancelablePromise<Value>;
export default _default;
export { Separator };
