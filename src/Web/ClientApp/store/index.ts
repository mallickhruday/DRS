﻿import { ActionCreatorGeneric } from 'redux-typed';
import { Dispatch } from 'redux';

import * as EntityStore from './entity';
import * as UserErrorStore from './UserError';
import * as UIStore from './ui';

export interface ApplicationState {
    userErrors: UserErrorStore.State;
    entities: EntityStore.State;
    ui: UIStore.State;
}

export const reducers = {
    userErrors: UserErrorStore.reducer
    , entities: EntityStore.reducer
    , ui: UIStore.reducer
};

export interface TypedActionCreatorGeneric<TState, TReturn> extends Function {
    (dispatch: Dispatch<TState>, getState: () => TState): TReturn;
};

export type TypedActionCreator<TReturn> = TypedActionCreatorGeneric<ApplicationState, TReturn>;