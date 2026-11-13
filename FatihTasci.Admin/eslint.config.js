import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'
import noDesignClasses from './eslint-rules/no-design-classes.js'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
  },
  // CLAUDE.md kural 10: ham <button>/<input> yok.
  // Gerçek bir istisna için bir satır üstüne gerekçeli disable yorumu eklenir:
  //   // eslint-disable-next-line no-restricted-syntax -- <neden gerekli>
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/components/ui/**'],
    rules: {
      'no-restricted-syntax': [
        'error',
        {
          selector: "JSXOpeningElement[name.name=/^(button|input)$/]",
          message:
            "Ham <button>/<input> kullanmayın; @/components/ui içindeki Button/Input'u kullanın (CLAUDE.md kural 10).",
        },
      ],
    },
  },
  // CLAUDE.md kural 18: shadcn bileşenlerine görünüm class'ı verilmez, yalnızca yerleşim class'ı serbesttir.
  // Gerçek bir istisna için: // eslint-disable-next-line gora/no-design-classes -- <neden>
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/components/ui/**'],
    plugins: { gora: { rules: { 'no-design-classes': noDesignClasses } } },
    rules: { 'gora/no-design-classes': 'error' },
  },
])
