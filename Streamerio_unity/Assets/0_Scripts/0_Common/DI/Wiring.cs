// モジュール概要:
// IAttachable 実装と VContainer のライフサイクルを接続し、Initialize/Dispose タイミングで Attach/Detach を自動実行する。
// 依存関係: VContainer の IInitializable / IDisposable を利用し、DI されたターゲットとコンテキストを保持する。
// 使用例: LifetimeScope.Configure で RegisterEntryPoint<Wiring<Presenter, Context>>() し、Presenter へ View などの依存を渡す。

using System;
using VContainer;
using VContainer.Unity;

namespace Common
{
    /// <summary>
    /// 【目的】<see cref="IInitializable"/> / <see cref="IDisposable"/> のライフサイクルに合わせて <see cref="IAttachable{TContext}"/> を自動接続する。
    /// 【理由】LifetimeScope 登録時に Attach/Detach の呼び忘れを防ぎ、Presenter 初期化とクリーンアップを一貫した手順へ揃えるため。
    /// </summary>
    /// <typeparam name="TAttachable">
    /// 【用途】<see cref="IAttachable{TContext}"/> を実装した接続対象。Presenter やサービスなど、スコープごとに初期化したいクラス。
    /// </typeparam>
    /// <typeparam name="TContext">
    /// 【用途】Attach 時に渡すコンテキスト情報の型。View や Model、初期化データなどを保持し、生成側と利用側を疎結合にする。
    /// </typeparam>
    public class Wiring<TAttachable, TContext> : IInitializable, IDisposable
        where TAttachable : IAttachable<TContext>
    {
        private readonly TAttachable _target;
        private readonly TContext _context;

        /// <summary>
        /// 【目的】Wiring の新しいインスタンスを作成し、Attach 対象とコンテキストを保持する。
        /// 【理由】エントリポイント登録時に一度だけ依存を受け取り、Initialize/Dispose で再利用するため。
        /// </summary>
        /// <param name="target">【用途】Attach/Detach の対象となるオブジェクト。null であってはならない。</param>
        /// <param name="context">【用途】Attach 時に渡すコンテキスト情報。View やサービスなど。</param>
        /// <exception cref="ArgumentNullException">target が null の場合に発生。</exception>
        public Wiring(TAttachable target, TContext context)
        {
            _target  = target ?? throw new ArgumentNullException(nameof(target));
            _context = context;
        }

        /// <summary>
        /// 【目的】<see cref="IInitializable.Initialize"/> のライフサイクルで Attach を呼び出し、依存を接続する。
        /// 【理由】VContainer がエントリポイントを起動した瞬間に Presenter の初期化が完了するようにするため。
        /// </summary>
        public void Initialize() => _target.Attach(_context);

        /// <summary>
        /// 【目的】<see cref="IDisposable.Dispose"/> のライフサイクルで Detach を呼び出し、リソースを解放する。
        /// 【理由】LifetimeScope 終了時にイベント購読や参照を確実に解除し、リークや例外を防ぐため。
        /// </summary>
        public void Dispose() => _target.Detach();
    }
}
