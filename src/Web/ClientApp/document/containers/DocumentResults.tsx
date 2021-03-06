﻿import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import { actionCreators as DocumentSearchActions } from '../../store/ui/DocumentSearch';
import { actionCreators as DocumentEditorActions } from '../../store/ui/DocumentEditor';
import { DocumentResults, DocumentResultsDispatchProps, DocumentResultsStateProps } from '../components';

const mapStateToProps = (state: ApplicationState): DocumentResultsStateProps => ({
    keywords: '' //state.search.keywords
    , libraryIds: [] // state.search.libraries.selectedIds
    , libraryOptions: [] // state.search.libraries.allIds.map(id => state.search.libraries.byId[id])
    , isFetching: state.ui.documentSearch.isSearching
    , documents: state.ui.documentSearch.allIds.map(id => state.entities.documents.byId[id])
    , selected: state.ui.documentSearch.selectedIds
    , nextPage: ''
});

const mapDispatchToProps = (dispatch: any): DocumentResultsDispatchProps => ({
    onDelete: (id: number) => dispatch(DocumentEditorActions.delete(id))
    , onEdit: (id: number) => dispatch(DocumentEditorActions.edit(id))
    , onSelect: (id: number) => dispatch(DocumentSearchActions.selectDocument(id))
});

export default connect(mapStateToProps, mapDispatchToProps)(DocumentResults);