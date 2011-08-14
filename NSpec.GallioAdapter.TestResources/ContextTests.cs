using NSpec;

namespace NSpec.GallioAdapter.TestResources
{
    public class before_defined_as_a_method : nspec
    {
        Calculator calculator;

        void before_each()
        {
            this.calculator = new Calculator();
        }

        void when_adding_numbers_with_the_calculator()
        {
            act = () => this.calculator.Add( 1 );

            it["should display 1"] = () =>
            {
                this.calculator.Display().should_be( 1 );
            };

            context["another value is added "] = () =>
            {
                act = () => this.calculator.Add( 1 );

                it["should display 2"] = () =>
                {
                    this.calculator.Display().should_be( 2 );
                };
            };
        }
    }

    //public class before_defined_inside_a_context_block : nspec
    //{
    //    Calculator calculator;

    //    void when_adding_numbers_with_the_calculator()
    //    {
    //        before = () =>
    //        {
    //            this.calculator = new Calculator();
    //        };

    //        act = () => this.calculator.Add( 1 );

    //        it["should display 1"] = () =>
    //        {
    //            this.calculator.Display().should_be( 1 );
    //        };

    //        context["another value is added "] = () =>
    //        {
    //            act = () => this.calculator.Add( 1 );

    //            it["should display 2"] = () =>
    //            {
    //                this.calculator.Display().should_be( 2 );
    //            };
    //        };
    //    }
    //}

    public class Calculator
    {
        int sum = 0;

        public void Add( int x )
        {
            sum += x;
        }

        public int Display()
        {
            return sum;
        }
    }
}