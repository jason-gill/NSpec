using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using HtmlTags;

namespace NSpec.Domain.Formatters
{
    [Serializable]
    public class DocumentStyleFormatter : IFormatter
    {
        #region Implementation of IFormatter

        public void Write( ContextCollection contexts )
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.Head.Children.Add( this.BuildInLineCss() );

            htmlDocument.Body.Children.Add( this.BuildTableOfSpecifications( contexts ) );

            Stack<string> breadcrumbs = new Stack<string>();
            breadcrumbs.Push( "ToS" );
            contexts.Do( c => this.BuildContext( c, htmlDocument.Body, breadcrumbs  ) );

            Console.WriteLine( htmlDocument.ToString() );
        }

        private void BuildContext( Context context, HtmlTag htmlBody, Stack<string> breadcrumbs )
        {
            HtmlTag contextPage = new HtmlTag( "div" ).Id( context.Name.RemoveWhiteSpace() ).AddClass( "context-page" );

            contextPage.Children.Add( this.BuildBreadcrumbs( breadcrumbs ) );
            contextPage.Children.Add( this.BuildAllContextsRunScore( context ) );
            contextPage.Children.Add( this.BuildContextName( context ) );

            if( context.IsGrouping )
            {
                contextPage.Children.Add( this.BuildRemainingContextsAndExamples( context ) );
                htmlBody.Children.Add( contextPage );
            }
            else
            {
                contextPage.Children.Add( this.BuildContextExamples( context.Examples ) );
                contextPage.Children.Add( this.BuildContextChildren( context ) );

                htmlBody.Children.Add( contextPage );

                breadcrumbs.Push( context.Name );
                context.Contexts.Do( c => this.BuildContext( c, htmlBody, breadcrumbs ) );
                breadcrumbs.Pop();
            }
        }

        HtmlTag BuildRemainingContextsAndExamples( Context context )
        {
            HtmlTag contextExamples = new HtmlTag( "div" ).AddClass( "context-examples" );

            if( context.Examples.Count > 0 )
            {
                HtmlTag ulContextExamples = new HtmlTag( "ul" );
                StringBuilder sbContextExamples = new StringBuilder();
                context.Examples.Do( e => this.BuildContextExample( sbContextExamples, e ) );
                ulContextExamples.Text( sbContextExamples.ToString() ).Encoded( false );

                contextExamples.Children.Add( ulContextExamples );
            }

            StringBuilder sbChildContextsAndExamples = new StringBuilder();
            context.Contexts.Do( c => this.BuildChildContextsAndExamples( sbChildContextsAndExamples, c ) );
            HtmlTag divChildContextsAndExamples = new HtmlTag( "div" );
            divChildContextsAndExamples.Text( sbChildContextsAndExamples.ToString() ).Encoded( false );

            contextExamples.Children.Add( divChildContextsAndExamples );

            return contextExamples;
        }

        #endregion

        private HtmlTag BuildTableOfSpecifications( ContextCollection contexts )
        {
            HtmlTag contextPage = new HtmlTag( "div" ).Id("ToS").AddClass( "context-page" );
            HtmlTag contextRunDate = new HtmlTag( "div" ).AddClass( "context-run-date" );
            HtmlTag tableOfSpecsHeading = new HtmlTag( "div" ).AddClass( "context-name" );
            HtmlTag parentContexts = new HtmlTag( "div" ).AddClass( "context-children" );

            contextRunDate.Text( DateTime.Now.ToString() );
            tableOfSpecsHeading.Children.Add( new HtmlTag("h1").Text("Table of Specifications") );
            
            parentContexts.Children.Add( this.BuildListOfContexts( contexts ) );

            contextPage.Children.Add( contextRunDate );
            contextPage.Children.Add( this.BuildAllContextsRunScore( contexts ) );
            contextPage.Children.Add( tableOfSpecsHeading );
            contextPage.Children.Add( parentContexts );

            return contextPage;
        }

        private HtmlTag BuildBreadcrumbs( Stack<string> breadcrumbs )
        {
            HtmlTag contextBreadcrumbs = new HtmlTag( "div" ).AddClass( "context-breadcrumbs" );

            foreach( string breadcrumb in breadcrumbs.Reverse() )
            {
                HtmlTag link = new HtmlTag( "a" )
                    .Attr( "href", String.Format( "#{0}", breadcrumb.RemoveWhiteSpace() ) )
                    .Attr( "target", "_top" )
                    .Text( breadcrumb );

                HtmlTag spacer = new HtmlTag( "span" ).AddClass( "context-breadcrumb-spacer" ).Text( ">" );

                contextBreadcrumbs.Children.Add( link );
                contextBreadcrumbs.Children.Add( spacer );
            }

            return contextBreadcrumbs;
        }

        private HtmlTag BuildAllContextsRunScore( IContextScore contextScore )
        {
            HtmlTag contextRunScore = new HtmlTag( "div" ).AddClass( "context-run-score" );
            
            StringBuilder score = new StringBuilder();
            score.AppendFormat( "Total:{0}", new HtmlTag( "span" )
                .Text( contextScore.AllExamples().Count().ToString() ).Encoded( false ) );
            score.AppendFormat( "Failed:{0}", new HtmlTag( "span" ).AddClass("spec-failed")
                .Text( contextScore.Failures().Count().ToString() ).Encoded( false ) );
            score.AppendFormat( "Pending:{0}", new HtmlTag( "span" ).AddClass("spec-pending")
                .Text( contextScore.Pendings().Count().ToString() ).Encoded( false ) );
            contextRunScore.Text( score.ToString() ).Encoded( false );

            return contextRunScore;
        }

        private HtmlTag BuildContextName( Context context )
        {
            HtmlTag contextName = new HtmlTag( "div" ).AddClass( "context-name" );
            contextName.Children.Add( new HtmlTag( "h1" ).Text( context.Name ) );

            return contextName;
        }

        private HtmlTag BuildListOfContexts( ContextCollection contexts )
        {
            HtmlTag tosTable = new HtmlTag( "table" );
            foreach( Context context in contexts )
            {
                HtmlTag row = new HtmlTag( "tr" );
                
                HtmlTag contextName = new HtmlTag( "td" );
                HtmlTag ahref = new HtmlTag( "a" )
                    .Attr( "href", String.Format( "#{0}", context.Name.RemoveWhiteSpace() ) )
                    .Attr( "target", "_top" )
                    .Text( context.Name );
                contextName.Children.Add( ahref );

                int failures = context.Failures().Count();
                int pendings = context.Pendings().Count();

                HtmlTag contextTotalSpecs = new HtmlTag( "td" ).Style( "text-align", "right" )
                    .Text( new HtmlTag( "span" ).AddClass( "spec-total" ).Text( context.AllExamples().Count().ToString() ).ToString() )
                    .Encoded( false );
                HtmlTag contextFailedSpecs = new HtmlTag( "td" ).Style( "text-align", "right" )
                    .Text( new HtmlTag( "span" ).AddClass( (failures == 0 ? "spec-failed" : "spec-failed-inverse") ).Text( failures.ToString() ).ToString() )
                    .Encoded( false );
                HtmlTag contextPendingSpecs = new HtmlTag( "td" ).Style( "text-align", "right" )
                    .Text( new HtmlTag( "span" ).AddClass( (pendings == 0 ? "spec-pending" : "spec-pending-inverse") ).Text( pendings.ToString() ).ToString() )
                    .Encoded( false );

                row.Children.Add( contextTotalSpecs ); 
                row.Children.Add( contextFailedSpecs ); 
                row.Children.Add( contextPendingSpecs );
                row.Children.Add( contextName );

                tosTable.Children.Add( row );
            }

            return tosTable;
        }

        private void BuildChildContextsAndExamples( StringBuilder sb, Context context )
        {
            sb.AppendLine( "<ul>" );
            sb.AppendFormat( "<li>{0}", context.Name );
            sb.AppendLine();
            if( context.Examples.Count > 0 )
            {
                sb.AppendLine( "<ul>" );
                context.Examples.Do( e => this.BuildContextExample( sb, e ) );
                sb.AppendLine( "</ul>" );
            }
            context.Contexts.Do( c => this.BuildChildContextsAndExamples( sb, c ) );
            sb.AppendLine();
            sb.AppendLine( "</li>" );
            sb.AppendLine( "</ul>" );
        }

        private HtmlTag BuildContextExamples( List<Example> examples )
        {
            HtmlTag contextExamples = new HtmlTag( "div" ).AddClass( "context-examples" );

            if( examples.Count > 0 )
            {
                HtmlTag ul = new HtmlTag( "ul" );
                StringBuilder sb = new StringBuilder();
                examples.Do( e => this.BuildContextExample( sb, e ) );
                ul.Text( sb.ToString() ).Encoded( false );

                contextExamples.Children.Add( ul );
            }
            else
            {
                contextExamples.Children.Add(
                    new HtmlTag( "span" ).Text( "No specifications defined." ) );
            }

            return contextExamples;
        }
        private void BuildContextExample( StringBuilder sb, Example example )
        {
            sb.AppendFormat( "<li>{0}", example.Spec );
            if( example.Exception != null )
            {
                sb.AppendLine( "<span class=\"spec-failed\">&lArr; Failed</span>" );
                sb.Append( "<div class=\"spec-exception\"><code>" );
                sb.Append( HttpUtility.HtmlEncode( example.Exception.ToString() ) );
                sb.AppendLine( "</code></div>" );
            }
            else if( example.Pending )
            {
                sb.Append( "<span class=\"spec-pending\">&lArr; Pending</span>" );
            }
            else
            {
                sb.Append( "<span class=\"spec-passed\">&lArr; Passed</span>" );
            }
            sb.AppendLine( "</li>" );
        }

        private HtmlTag BuildContextChildren( Context context )
        {
            
            HtmlTag contextChildren = new HtmlTag( "div" ).AddClass( "context-children" );

            if( context.Contexts.Count > 0 )
            {
                contextChildren.Children.Add( new HtmlTag( "h2" ).Text( "Additional specifications" ) );
                contextChildren.Children.Add( this.BuildListOfContexts( context.Contexts ) );
            }

            return contextChildren;
        }

        private HtmlTag BuildInLineCss()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine( "span { padding: 0.5em; }" );
            sb.AppendLine( "ul { margin-top: 0px; }" );

            sb.AppendLine( "h1 { font-size: 2em; }" );
            sb.AppendLine( "h2 { font-size: 1.25em; }" );

            sb.AppendLine( ".context-page" );
            sb.AppendLine( "{" );
            sb.AppendLine( "  width: 960px;" );
            sb.AppendLine( "  border: thin solid #000000;" );
            sb.AppendLine( "  margin: 0px auto 2em auto;" );
            sb.AppendLine( "  padding: 1em; " );
            sb.AppendLine( "  /* Added a shadow effect, this doesn't work in IE7 or IE8 */" );
            sb.AppendLine( "  -moz-box-shadow: 10px 10px 5px #888; /* FF3.5 - 3.6 */" );
            sb.AppendLine( "  -webkit-box-shadow: 10px 10px 5px #888; /* Saf3.0+, Chrome */" );
            sb.AppendLine( "  box-shadow: 10px 10px 5px #888; /* Opera 10.5, IE9, FF4+, Chrome 10+ */" );
            sb.AppendLine( "}" );

            sb.AppendLine( ".context-run-score { text-align: left; font-weight: bold; }" );
            sb.AppendLine( ".context-run-date { text-align: left; color: gray; font-size: x-small; }" );
            sb.AppendLine( ".context-breadcrumbs { padding: 0 0 1em 0; }" );
            sb.AppendLine( ".context-breadcrumb-spacer { font-size: .75em; }" );
            sb.AppendLine( ".context-examples { background-color: #F5FAFA; border-color: #ACD1E9; border-style: solid; border-width: thin; margin-bottom: 1em; padding-right: 1em; }" );

            sb.AppendLine( ".spec-total { font-weight: bold; color: #000000; }" );
            sb.AppendLine( ".spec-passed { font-weight:bold; color:green; }" );
            sb.AppendLine( ".spec-failed { font-weight: bold; color: #FF0000; }" );
            sb.AppendLine( ".spec-failed-inverse { font-weight: bold; color: #FFFFFF; background-color: #FF0000; }" );
            sb.AppendLine( ".spec-pending { font-weight: bold; color: #FF9900; }" );
            sb.AppendLine( ".spec-pending-inverse { font-weight: bold; color: #FFFFFF; background-color: #FF9900; }" );
			sb.AppendLine( ".spec-exception {background-color: #FFD2CF; border-color: #FF828D; border-style: dashed; border-width: thin; padding: 1em; font-size:small; white-space: pre-wrap; padding: 0.5em}" );

            HtmlTag style = new HtmlTag( "style" );
            style.Attr( "type", "text/css" );
            style.Text( sb.ToString() );

            return style;
        }
    }
}