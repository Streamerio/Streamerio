// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// This file provides utility types, assertion functions, and helper functions
// (like for API pagination and error handling) used across the core modules
// of clasp.
import Debug from 'debug';
import { GaxiosError } from 'googleapis-common';
const debug = Debug('clasp:core');
/**
 * Asserts that the provided ClaspOptions include credentials.
 * Throws an error if credentials are not set. This also acts as a type guard.
 * @param {ClaspOptions} options - The Clasp options to check.
 * @throws {Error} If `options.credentials` is not set.
 */
export function assertAuthenticated(options) {
    if (!options.credentials) {
        debug('Credentials not set in options: %O', options);
        throw new Error('No credentials found.', {
            cause: {
                code: 'NO_CREDENTIALS',
            },
        });
    }
}
/**
 * Asserts that the provided ClaspOptions include essential script project configurations.
 * Throws an error if `scriptId`, `projectRootDir`, `configFilePath`, or `contentDir` are missing.
 * This also acts as a type guard.
 * @param {ClaspOptions} options - The Clasp options to check.
 * @throws {Error} If essential script configurations are missing.
 */
export function assertScriptConfigured(options) {
    var _a;
    if (!((_a = options.project) === null || _a === void 0 ? void 0 : _a.scriptId) ||
        !options.files.projectRootDir ||
        !options.configFilePath ||
        !options.files.contentDir) {
        debug('Script configuration not found in options: %O', options);
        throw new Error('Project settings not found.', {
            cause: {
                code: 'MISSING_SCRIPT_CONFIGURATION',
            },
        });
    }
}
/**
 * Asserts that the provided ClaspOptions include a GCP project ID, in addition to base script configurations.
 * Throws an error if `projectId` is missing. This also acts as a type guard.
 * @param {ClaspOptions} options - The Clasp options to check.
 * @throws {Error} If `options.project.projectId` is not set.
 */
export function assertGcpProjectConfigured(options) {
    var _a;
    assertScriptConfigured(options);
    if (!((_a = options.project) === null || _a === void 0 ? void 0 : _a.projectId)) {
        debug('Script configuration not found in options: %O', options);
        throw new Error('Project ID not found.', {
            cause: {
                code: 'MISSING_PROJECT_ID',
            },
        });
    }
}
function pageOptionsWithDefaults(options) {
    return {
        pageSize: 100,
        maxPages: 10,
        maxResults: Number.MAX_SAFE_INTEGER,
        ...(options !== null && options !== void 0 ? options : {}),
    };
}
export async function fetchWithPages(fn, options) {
    const { pageSize, maxPages, maxResults } = pageOptionsWithDefaults(options);
    let pageToken = undefined;
    let pageCount = 0;
    const results = [];
    do {
        debug('Fetching page %d', pageCount + 1);
        const page = await fn(pageSize, pageToken);
        if (page.results) {
            results.push(...page.results);
        }
        ++pageCount;
        pageToken = page.pageToken;
    } while (pageToken && pageCount < maxPages && results.length < maxResults);
    if (pageToken) {
        debug('Returning partial results, page limit exceeded');
    }
    if (results.length > maxResults) {
        debug('Trimming results to %d', maxResults);
        return {
            results: results.slice(0, maxResults),
            partialResults: true,
        };
    }
    return {
        results,
        partialResults: pageToken !== undefined,
    };
}
/**
 * Checks if an error object is a GaxiosError with detailed error information.
 * @param {unknown} error - The error object to check.
 * @returns {boolean} True if the error is a GaxiosError with details, false otherwise.
 */
function isDetailedError(error) {
    if (!error) {
        return false;
    }
    const detailedError = error;
    if (detailedError.errors === undefined) {
        return false;
    }
    if (detailedError.errors.length === 0) {
        return false;
    }
    return true;
}
const ERROR_CODES = {
    400: 'INVALID_ARGUMENT',
    401: 'NOT_AUTHENTICATED',
    403: 'NOT_AUTHORIZED',
    404: 'NOT_FOUND',
};
/**
 * Standardized error handler for Google API errors (GaxiosError).
 * It extracts a meaningful message and error code, then re-throws a new error.
 * @param {unknown} error - The error received from a Google API call.
 * @throws {Error} A new error with a structured cause including the original error,
 * a clasp-specific error code, and the message.
 */
export function handleApiError(error) {
    var _a;
    debug('Handling API error: %O', error);
    if (!(error instanceof GaxiosError)) {
        throw new Error('Unexpected error', {
            cause: {
                code: 'UNEXPECTED_ERROR',
                message: new String(error),
                error: error,
            },
        });
    }
    const status = error.status;
    let message = error.message;
    if (isDetailedError(error)) {
        message = error.errors[0].message;
    }
    const code = (_a = ERROR_CODES[status !== null && status !== void 0 ? status : 0]) !== null && _a !== void 0 ? _a : 'UNEXPECTED_API_ERROR';
    throw new Error(message, {
        cause: {
            code: code,
            message: message,
            error: error,
        },
    });
}
/**
 * Ensures that a value is an array of strings.
 * If the input is a single string, it's wrapped in an array.
 * If it's already an array of strings, it's returned as is.
 * If it's an array containing non-string elements, those elements are filtered out.
 * If the input is neither a string nor an array, an empty array is returned.
 * @param {string | string[]} value - The value to process.
 * @returns {string[]} An array of strings.
 */
export function ensureStringArray(value) {
    if (typeof value === 'string') {
        return [value];
    }
    else if (Array.isArray(value)) {
        // Ensure all elements in the array are strings.
        if (value.every(item => typeof item === 'string')) {
            return value;
        }
        else {
            // Handle cases where the array contains non-string elements.
            // You could throw an error, filter out non-strings, or convert them to strings.
            // Example: filter out non-strings
            return value.filter(item => typeof item === 'string');
        }
    }
    else {
        // Handle cases where the value is neither a string nor an array of strings.
        // You could throw an error or return an empty array.
        // Example: return an empty array
        return [];
    }
}
